namespace Server.Model;

public sealed class Player
{
    public const int MovementSpeed = 5;
    
    public required Guid Id { get; init; }
    public required Room? Room { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public byte? AngleOfMovement { get; set; }
    public bool IsMoving => AngleOfMovement.HasValue;
    
    public DateTimeOffset LastHeardFrom { get; set; } = DateTimeOffset.UtcNow;
}