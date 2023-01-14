namespace Shared;

public record ServerCommand(byte Type, IServerCommandData Data)
{
    public const int PlayerUpdate = 1;

    public byte[] ToByteArray()
    {
        using var stream = new MemoryStream(1);
        using var writer = new BinaryWriter(stream);

        writer.Write(Type);
        writer.Write(Data.ToByteArray());
        
        return stream.ToArray();
    }
}

public interface IServerCommandData
{
    Span<byte> ToByteArray();
}

public record PlayerUpdateServerCommand(List<PlayerState> Players) : IServerCommandData
{
    public Span<byte> ToByteArray()
    {
        var bytes = new byte[Players.Count * 12];
        var byteSpan = (Span<byte>)bytes;
        
        for(int i = 0; i < Players.Count; i++)
            Players[i].ToByteArray().CopyTo(byteSpan.Slice(i * 12, 12));

        return bytes;
    }
}