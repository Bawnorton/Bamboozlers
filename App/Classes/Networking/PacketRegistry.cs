namespace Bamboozlers.Classes.Networking;

public class PacketRegistry
{
    public static readonly PacketRegistry Instance = new();

    private readonly Dictionary<Type, int> _typeToId = new();
    private readonly Dictionary<int, Type> _idToType = new();
    
    private PacketRegistry()
    {
        // RegisterType(typeof(ReadDatabaseC2SPacket), 0);
    }

    private void RegisterType(Type type, int id)
    {
        if (!typeof(IPacket).IsAssignableFrom(type))
        {
            throw new ArgumentException($"Packet Type {type} does not implement IPacket");
        }
        if (!_typeToId.TryAdd(type, id))
        {
            throw new ArgumentException($"Packet Type {type} is already registered with id {_typeToId[type]}");
        }
        if (!_idToType.TryAdd(id, type))
        {
            throw new ArgumentException($"Packet Id {id} is already registered with type {_idToType[id]}");
        }
    }
    
    public int GetIdForType(Type type)
    {
        if (!_typeToId.TryGetValue(type, out var id))
        {
            throw new ArgumentException($"Packet Type {type} is not registered");
        }
        return id;
    }
    
    public Type GetTypeForId(int id)
    {
        if (!_idToType.TryGetValue(id, out var type))
        {
            throw new ArgumentException($"Packet Id {id} is not registered");
        }
        return type;
    }
}