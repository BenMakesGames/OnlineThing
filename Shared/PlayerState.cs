using System.Diagnostics.CodeAnalysis;

namespace Shared;

public record PlayerState(float X, float Y, byte? AngleOfMovement)
{
    public Span<byte> ToByteArray()
    {
        if(AngleOfMovement.HasValue)
        {
            var bytes = new byte[9];
            var byteSpan = (Span<byte>)bytes;
        
            BitConverter.TryWriteBytes(byteSpan, X);
            BitConverter.TryWriteBytes(byteSpan[4..], Y);
            byteSpan[8] = AngleOfMovement.Value;

            return bytes;
        }
        else
        {
            var bytes = new byte[8];
            var byteSpan = (Span<byte>)bytes;
            
            BitConverter.TryWriteBytes(byteSpan, X);
            BitConverter.TryWriteBytes(byteSpan[4..], Y);

            return bytes;
        }
    }
    
    public static bool TryParse(Span<byte> bytes, [MaybeNullWhen(false)] out PlayerState playerState)
    {
        if(bytes.Length is < 8 or > 9)
        {
            playerState = default;
            return false;
        }
        
        var x = BitConverter.ToSingle(bytes);
        var y = BitConverter.ToSingle(bytes[4..]);

        playerState = bytes.Length == 9
            ? new PlayerState(x, y, bytes[8])
            : new PlayerState(x, y, null)
        ;

        return true;
    }
}
