namespace Bamboozlers.Classes.Networking;

public interface IPacket
{
    public int? GetSenderId();
    public Type PacketType();

    public string AsJson();
    public void FromJson(string json);
}