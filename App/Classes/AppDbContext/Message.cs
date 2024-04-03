using System.ComponentModel.DataAnnotations;

namespace Bamboozlers.Classes.AppDbContext;

public class Message
{
    [Key] public int ID { get; set; }

    public Chat? Chat { get; set; }
    public int ChatID { get; set; }
    public User? Sender { get; set; }
    public int SenderID { get; set; }
    public string Content { get; set; }
    public byte[]? Attachment { get; set; }
    public bool IsPinned { get; set; }
    public DateTime SentAt { get; set; }
}