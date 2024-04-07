using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Bamboozlers.Classes.AppDbContext;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
public class Message
{
    [Key] public int ID { get; set; }

    public Chat? Chat { get; set; }
    public int ChatID { get; set; }
    public User? Sender { get; set; }
    public int SenderID { get; set; }
    public string Content { get; set; } = default!;
    public byte[]? Attachment { get; set; }
    public bool IsPinned { get; set; }
    public DateTime SentAt { get; set; }
}