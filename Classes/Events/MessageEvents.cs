using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Events.Api;
using Bamboozlers.Classes.Func;

namespace Bamboozlers.Classes.Events;

public abstract class MessageEvents
{
    public static readonly Event<MessageSentEvent> MessageSent = EventFactory.CreateArrayBacked<MessageSentEvent>(listeners =>
        async (chat, sender, content, attachment, isPinned, sentAt, _, _) =>
        {
            var message = new Message
            {
                ChatID = chat.ID,
                SenderID = sender.Id,
                Content = content,
                // Attachment = attachment,
                IsPinned = isPinned,
                SentAt = sentAt
            };
            var mutatedMessage = message;
            foreach (var listener in listeners)
            {
                mutatedMessage = await listener(chat, sender, content, attachment, isPinned, sentAt, () => message, message.ID);
            }

            return mutatedMessage;
        });
    
    public static readonly Event<MessageEditedEvent> MessageEdited = EventFactory.CreateArrayBacked<MessageEditedEvent>(listeners =>
        async (id, chat, editor, oldContent, newContent, attachment, wasPinned, isPinned, editedAt) =>
        {
            foreach (var listener in listeners)
            {
                await listener(id, chat, editor, oldContent, newContent, attachment, wasPinned, isPinned, editedAt);
            }
        });
    
    public static readonly Event<MessageDeletedEvent> MessageDeleted = EventFactory.CreateArrayBacked<MessageDeletedEvent>(listeners =>
        async (id, chat, deleter, deletedContent, removedAttachment, wasPinned, deletedAt) =>
        {
            foreach (var listener in listeners)
            {
                await listener(id, chat, deleter, deletedContent, removedAttachment, wasPinned, deletedAt);
            }
        });
    
    public delegate Task<Message?> MessageSentEvent(Chat chat, User sender, string content, string? attachment, bool isPinned, DateTime sentAt, Supplier<Message>? createdMessageSupplier = default, int createdId = default);
    
    public delegate Task MessageEditedEvent(int id, Chat chat, User editor, string oldContent, string newContent, string? attachment, bool wasPinned, bool isPinned, DateTime editedAt);
    
    public delegate Task MessageDeletedEvent(int id, Chat chat, User deleter, string deletedContent, string? removedAttachment, bool wasPinned, DateTime deletedAt);
}