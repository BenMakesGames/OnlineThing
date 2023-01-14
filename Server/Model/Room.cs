namespace Server.Model;

public sealed class Room
{
    public Guid Id { get; } = Guid.NewGuid();
    public List<Player> Players { get; } = new();
}