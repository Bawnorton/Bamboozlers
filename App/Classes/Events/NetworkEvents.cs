using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Events.Api;

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
    
    public static readonly Event<TellOthersToReadDatabaseRequestEvent> TellOthersToReadDatabaseRequest = EventFactory.CreateArrayBacked<TellOthersToReadDatabaseRequestEvent>(listeners => async (senderId, chatId, dbEntry) =>
    {
        foreach (var listener in listeners)
        {
            await listener(senderId, chatId, dbEntry);
        }
    });

    public delegate Task ReadDatabaseRequestEvent(DbEntry type);
    
    public delegate Task TellOthersToReadDatabaseRequestEvent(int senderId, int chatId, DbEntry dbEntry);
}