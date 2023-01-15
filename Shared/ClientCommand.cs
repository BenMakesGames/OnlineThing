using System.Diagnostics.CodeAnalysis;

namespace Shared;

public record ClientCommand(Guid PlayerId, byte Type, IClientCommandData Data)
{
    public const byte StopMoving = 1;
    public const byte StartMoving = 2;
    public const byte Disconnect = 250;
    
    public static bool TryParse(Span<byte> bytes, [MaybeNullWhen(false)] out ClientCommand command)
    {
        command = null;

        if(bytes.Length < 17)
            return false;

        var playerId = new Guid(bytes[..16]);
        var type = bytes[16];
        IClientCommandData data = new InvalidClientCommand();

        var parsedData = type switch
        {
            StopMoving => StopMovingClientCommand.TryParse(bytes[17..], out data),
            StartMoving => StartMovingClientCommand.TryParse(bytes[17..], out data),
            Disconnect => DisconnectClientCommand.TryParse(bytes[17..], out data),
            _ => false
        };
        
        if(!parsedData)
            return false;

        command = new ClientCommand(playerId, type, data);

        return true;
    }

    public byte[] ToByteArray()
    {
        var data = Data.ToByteArray();
        var bytes = new byte[17 + data.Length];
        var byteSpan = (Span<byte>) bytes;
        PlayerId.TryWriteBytes(bytes);
        bytes[16] = Type;
        data.CopyTo(byteSpan[17..]);
        return bytes;
    }
}

public interface IClientCommandData
{
    static abstract byte Type { get; }
    byte[] ToByteArray();

    // some commands have empty data; for such commands, this method can be used
    protected static bool ParseEmpty<T>(Span<byte> bytes, out IClientCommandData data) where T : IClientCommandData, new()
    {
        if(bytes.Length != 0)
        {
            data = new InvalidClientCommand();

            return false;
        }

        data = new T();

        return true;
    }
}

public sealed record InvalidClientCommand : IClientCommandData
{
    public static byte Type => 0;

    public byte[] ToByteArray() => Array.Empty<byte>();
}

public sealed record DisconnectClientCommand: IClientCommandData
{
    public static byte Type => ClientCommand.Disconnect;

    public static bool TryParse(Span<byte> bytes, out IClientCommandData clientCommand)
        => IClientCommandData.ParseEmpty<DisconnectClientCommand>(bytes, out clientCommand);

    public byte[] ToByteArray() => Array.Empty<byte>();
}

public sealed record StopMovingClientCommand: IClientCommandData
{
    public static byte Type => ClientCommand.StopMoving;

    public static bool TryParse(Span<byte> bytes, out IClientCommandData clientCommand)
        => IClientCommandData.ParseEmpty<StopMovingClientCommand>(bytes, out clientCommand);

    public byte[] ToByteArray() => Array.Empty<byte>();
}

public sealed record StartMovingClientCommand(byte Angle) : IClientCommandData
{
    public static byte Type => ClientCommand.StartMoving;

    public static bool TryParse(Span<byte> bytes, out IClientCommandData clientCommand)
    {
        if(bytes.Length != 1)
        {
            clientCommand = new InvalidClientCommand();

            return false;
        }

        clientCommand = new StartMovingClientCommand(bytes[0]);

        return true;
    }

    public byte[] ToByteArray() => new[] { Angle };
}