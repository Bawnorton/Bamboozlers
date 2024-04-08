using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.AppDbContext;

[PrimaryKey(nameof(UserId),nameof(ChatId))]
[SuppressMessage("ReSharper", "InconsistentNaming")]

public class ChatUser
{
    public User User { get; set; } = default!;
    public int UserId { get; set; }

    public Chat Chat { get; set; } = default!;
    public int ChatId { get; set; }
    public DateTime LastAccess { get; set; }
}