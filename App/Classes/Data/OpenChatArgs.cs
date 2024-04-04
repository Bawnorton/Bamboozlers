namespace Bamboozlers.Classes.Data;

public class OpenChatArgs(ChatType type, int identifier)
{
    public ChatType ChatType { get; set; } = type;
    public int Id { get; set; } = identifier;
}

public enum ChatType
{
    Dm,
    Group
}