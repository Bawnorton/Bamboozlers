using System.ComponentModel.DataAnnotations;

namespace Bamboozlers.Classes.AppDbContext;

public class TestTable
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
}