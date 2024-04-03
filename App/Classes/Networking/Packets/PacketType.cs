using System.Text.Json;

namespace Bamboozlers.Classes.Networking.Packets;

public sealed class PacketType<T> : PacketType where T : IPacket
{
    private readonly Func<JsonElement, T>? _deserializer;
    private readonly string _id;

    private PacketType(string id, Func<JsonElement, T>? deserializer)
    {
        _id = id;
        _deserializer = deserializer;
    }

    public static PacketType<T> Create(string id, Func<JsonElement, T>? deserializer = default)
    {
        return new PacketType<T>(id, deserializer);
    }

    public override string GetId()
    {
        return _id;
    }

    public override object Read(JsonElement json)
    {
        if (_deserializer == null)
            throw new Exception(
                $"No deserializer provided for packet of type {typeof(T).Name}. It is likely serverbound");
        try
        {
            return _deserializer(json);
        }
        catch (Exception e)
        {
            throw new Exception($"Error while deserializing packet of type {typeof(T).Name}. JSON: {json}", e);
        }
    }
}

public abstract class PacketType
{
    public abstract string GetId();

    public abstract object Read(JsonElement obj);
}