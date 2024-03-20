using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Events.Api;

namespace Bamboozlers.Classes.Events;

public abstract class MessageEvents
{
    public static readonly Event<MessageCreatedEvent> MessageCreated = EventFactory.CreateArrayBacked<MessageCreatedEvent>(listeners =>
        async message =>
        {
            var mutatedMessage = message;
            foreach (var listener in listeners)
            {
                mutatedMessage = await listener(mutatedMessage);
                if (mutatedMessage is null)
                {
                    break;
                }
            }

            return mutatedMessage;
        });
    
    public static readonly Event<MessageEditedEvent> MessageEdited = EventFactory.CreateArrayBacked<MessageEditedEvent>(listeners =>
        async (oldMessage, newMessage, editor) =>
        {
            foreach (var listener in listeners)
            {
                await listener(oldMessage, newMessage, editor);
            }
        });
    
    public static readonly Event<MessageDeletedEvent> MessageDeleted = EventFactory.CreateArrayBacked<MessageDeletedEvent>(listeners =>
        async (message, deleter) =>
        {
            foreach (var listener in listeners)
            {
                await listener(message, deleter);
            }
        });
    
    public delegate Task<Message?> MessageCreatedEvent(Message message);
    
    public delegate Task MessageEditedEvent(Message oldMessage, Message newMessage, User editor);
    
    public delegate Task MessageDeletedEvent(Message message, User deleter);
}