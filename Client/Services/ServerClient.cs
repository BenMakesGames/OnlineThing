using System.Net;
using System.Net.Sockets;
using BenMakesGames.PlayPlayMini.Attributes.DI;
using Shared;

namespace Client.Services;

[AutoRegister(Lifetime.Singleton)]
public sealed class ServerClient
{
    public const int PortNumber = 5000;

    private IPEndPoint ServerEndPoint { get; }

    private UdpClient Client { get; }

    public ServerClient()
    {
        ServerEndPoint = new(IPAddress.Parse("127.0.0.1"), PortNumber);

        Client = new UdpClient(ServerEndPoint);
    }

    public void Send<T>(Guid playerId, T data) where T: IClientCommandData
    {
        var command = new ClientCommand(playerId, T.Type, data);
        Client.Send(command.ToByteArray());
    }

}