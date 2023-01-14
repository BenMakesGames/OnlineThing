namespace Server.Model;

public sealed class World
{
    public Dictionary<Guid, Room> Rooms { get; } = new();
    public Dictionary<Guid, Player> Players { get; } = new();
}