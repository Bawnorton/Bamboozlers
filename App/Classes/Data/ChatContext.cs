using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Func;

namespace Bamboozlers.Classes.Data;

public record ChatContext(
    bool IsMod,
    AsyncConsumer<Message> SetReplying,
    AsyncConsumer<Message> OnDelete,
    AsyncConsumer<Message> OnEditStart,
    AsyncConsumer<string?> SetLastEdit,
    AsyncConsumer<bool> OnEditEnd,
    AsyncConsumer<Message> OnPin);