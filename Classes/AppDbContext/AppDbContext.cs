namespace Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;
public class AppDbContext:DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public required DbSet<RenamedTestTable> RenamedTestTable { get; set; }
    public required DbSet<DirectMessageEntry> DirectMessageEntries { get; set; }
    public required DbSet<GroupEntry> GroupEntries { get; set; }
    public required DbSet<User> Users { get; set; }
}