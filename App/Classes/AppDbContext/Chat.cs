using System.ComponentModel.DataAnnotations;

namespace Bamboozlers.Classes.AppDbContext;

public class Chat
{
    [Key] public int ID { get; set; }

    public ICollection<User> Users { get; set; }
    public ICollection<Message>? Messages { get; set; }
}