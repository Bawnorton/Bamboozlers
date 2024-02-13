using System.ComponentModel.DataAnnotations;

namespace Bamboozlers.Classes.AppDbContext;

public class User
{
    [Key]
    public required int UserId { get; set; }
    
    [MaxLength(50)]
    public required string Username { get; set; }
    
    [MaxLength(200)]
    public required string Avatar { get; set; }
}