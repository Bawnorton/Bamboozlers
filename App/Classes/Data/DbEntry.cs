namespace Bamboozlers.Classes.Data;

public sealed class DbEntry
{
    public static readonly DbEntry ChatMessage = Create("chat_message");
    
    private readonly string _id;
    
    private DbEntry(string id)
    {
        _id = id;
    }
    
    public string GetId()
    {
        return _id;
    }

    private static DbEntry Create(string id)
    {
        return new DbEntry(id);
    }
    
    public static DbEntry FromId(string id)
    {
        return id switch
        {
            "chat_message" => ChatMessage,
            _ => throw new Exception("Invalid data type")
        };
    }
}