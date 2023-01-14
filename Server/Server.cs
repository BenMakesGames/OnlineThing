using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Server.Model;
using Shared;

namespace Server;

public sealed class Server: IDisposable
{
    public const int EstimatedMsToHandleOneRequest = 1;
    public const int WorldTicksPerSecond = 20;
    public const int PortNumber = 5000;
    public const int MaxMinutesOfPlayerInactivityBeforeBooting = 15;
    public const int UpdateRoomsMaxDegreeOfParallelism = -1;

    private ParallelOptions UpdateRoomsParallelOptions { get; } = new() { MaxDegreeOfParallelism = UpdateRoomsMaxDegreeOfParallelism };

    private World World { get; } = new();
    private bool IsRunning { get; set; } = true;
    
    private ConcurrentBag<Player> PlayersToBoot { get; } = new();

    private UdpClient UdpServer { get; }
    
    private Dictionary<Guid, IPEndPoint> ClientEndPoints { get; } = new();

    public Server()
    {
        UdpServer = new UdpClient(PortNumber);
    }
    
    public void Dispose()
    {
        UdpServer.Dispose();
    }

    public void Run()
    {
        // TODO: more robust time-keeping that attempts to catch up world updates if previous updates were slow
        var sw = Stopwatch.StartNew();
        long timeDebt = 0;
        
        while (IsRunning)
        {
            // update the world
            var maxPlayerAge = DateTimeOffset.UtcNow.AddMinutes(-MaxMinutesOfPlayerInactivityBeforeBooting);
            Parallel.ForEach(World.Rooms.Values, UpdateRoomsParallelOptions, r => UpdateRoom(r, maxPlayerAge));

            // handle as many requests as we can in the time remaining
            while(UdpServer.Available > 0 && (1000 / WorldTicksPerSecond) - sw.ElapsedMilliseconds >= EstimatedMsToHandleOneRequest)
            {
                var remoteEndPoint = new IPEndPoint(IPAddress.Any, PortNumber);
                HandleRequest(UdpServer.Receive(ref remoteEndPoint), remoteEndPoint);
            }

            // boot players that need booting
            BootPlayersToBoot();

            // if we have time left, sleep for the remaining time
            var timeRemaining = (1000 / WorldTicksPerSecond) - sw.ElapsedMilliseconds;
            
            if (timeRemaining > 0)
                Thread.Sleep((int)timeRemaining);
            else
                timeDebt += timeRemaining;

            sw.Restart();
        }        
    }

    private void HandleRequest(Span<byte> bytes, IPEndPoint clientEndPoint)
    {
        if(!ClientCommand.TryParse(bytes, out var command))
            return;

        if (!World.Players.TryGetValue(command.PlayerId, out var player))
        {
            var room = World.Rooms.Values.First();
            
            player = new Player()
            {
                Id = command.PlayerId,
                Room = room,
            };
            
            World.Players.Add(command.PlayerId, player);
            room.Players.Add(player);
            
            ClientEndPoints.Add(player.Id, clientEndPoint);
        }
        
        player.LastHeardFrom = DateTimeOffset.UtcNow;

        switch(command.Data)
        {
            case StopMovingClientCommand: player.AngleOfMovement = null; break;
            case StartMovingClientCommand moveCommand: player.AngleOfMovement = moveCommand.Angle; break;
            case DisconnectClientCommand: PlayersToBoot.Add(player); break;
        }
    }

    private void UpdateRoom(Room room, DateTimeOffset oldestTime)
    {
        var playerStates = new List<PlayerState>();
        
        foreach(var player in room.Players)
        {
            if(player.LastHeardFrom < oldestTime)
            {
                room.Players.Remove(player);
                player.Room = null;
                PlayersToBoot.Add(player);
                continue;
            }
            
            if (player.AngleOfMovement.HasValue)
            {
                player.X += (float)Math.Cos(player.AngleOfMovement.Value * Player.MovementSpeed);
                player.Y += (float)Math.Sin(player.AngleOfMovement.Value * Player.MovementSpeed);
            }
            
            playerStates.Add(new(player.X, player.X, player.AngleOfMovement));
        }

        var bytes = new ServerCommand(ServerCommand.PlayerUpdate, new PlayerUpdateServerCommand(playerStates)).ToByteArray();

        // ReSharper disable once ForCanBeConvertedToForeach
        for(var i = 0; i < room.Players.Count; i++)
            UdpServer.Send(bytes, ClientEndPoints[room.Players[i].Id]);
    }

    private void BootPlayersToBoot()
    {
        foreach(var p in PlayersToBoot)
        {
            p.Room?.Players.Remove(p);        
            World.Players.Remove(p.Id);
            ClientEndPoints.Remove(p.Id);
        }
            
        PlayersToBoot.Clear();
    }
}