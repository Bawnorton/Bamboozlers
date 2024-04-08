using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.AppDbContext;

[PrimaryKey(nameof(User1ID), nameof(User2ID))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Friendship(int User1ID, int User2ID)
{
    public int User1ID { get; set; } = User1ID;
    public User User1 { get; set; } = default!;
    public int User2ID { get; set; } = User2ID;
    public User User2 { get; set; } = default!;
}