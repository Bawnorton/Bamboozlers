using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Func;

namespace Bamboozlers.Classes.Data;

public record ChatContextData(bool IsShiftHelf, bool IsMod, AsyncConsumer<Message> OnDelete, AsyncBiConsumer<Message, string> OnEdit);