namespace Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;
public class AppDbContext:DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<TestTable> TestTable { get; set; }
}