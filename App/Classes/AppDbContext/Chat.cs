using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bamboozlers.Classes.AppDbContext;

public class Chat
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }
    public ICollection<User> Users { get; set; }
    public ICollection<Message>? Messages { get; set; }
}