using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Func;

namespace Bamboozlers.Classes.Data;

public record ChatContext(
    bool IsMod,
    AsyncConsumer<Message> OnDelete,
    AsyncConsumer<Message> OnEditStart,
    AsyncConsumer<bool> OnEditEnd,
    AsyncConsumer<Message> OnPin)
{
    public static string? LastEdit { get; set; }
}