using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Events.Api;
using Bamboozlers.Classes.Networking.Packets.Serverbound;

namespace Bamboozlers.Classes.Events;

public abstract class NetworkEvents
{
    public static readonly Event<ReadDatabaseRequestEvent> ReadDatabaseRequest = EventFactory.CreateArrayBacked<ReadDatabaseRequestEvent>(listeners => async entry =>
    {
        foreach (var listener in listeners)
        {
            await listener(entry);
        }
    });

    public delegate Task ReadDatabaseRequestEvent(DbEntry type);
}