using System.ComponentModel.DataAnnotations;

namespace Bamboozlers.Classes.AppDbContext;

public class GroupEntry
{
    [Key]
    public required int GroupId { get; set; }

    [MaxLength(50)]
    public required string GroupName { get; set; }
    
    [MaxLength(200)]
    public string? GroupAvatar { get; set; }
}