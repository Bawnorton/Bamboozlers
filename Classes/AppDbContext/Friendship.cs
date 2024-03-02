using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.AppDbContext;

[PrimaryKey(nameof(User1ID), nameof(User2ID))]
public class Friendship
{
    public int User1ID { get; set; }
    public User User1 { get; set; }
    public int User2ID { get; set; }
    public User User2 { get; set; }
}