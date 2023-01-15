namespace Client.Models;

public sealed class Player
{
    public float X { get; set; }
    public float Y { get; set; }
    public int PixelX => (int) X;
    public int PixelY => (int) Y;
    public byte? AngleOfMovement { get; set; }
}