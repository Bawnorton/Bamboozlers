using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.AppDbContext;

[PrimaryKey(nameof(BlockedID), nameof(BlockerID))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Block
{
    public Block(int BlockedID, int BlockerID)
    {
        this.BlockedID = BlockedID;
        this.BlockerID = BlockerID;
    }
    public int BlockerID { get; set; }
    public User Blocker { get; set; } = default!;
    public int BlockedID { get; set; }
    public User Blocked { get; set; } = default!;
}