using System.ComponentModel.DataAnnotations;

namespace Bamboozlers.Classes.AppDbContext;

public class RenamedTestTable
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
}