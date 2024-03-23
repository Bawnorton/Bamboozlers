using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;

public class AppDbContext:IdentityDbContext<User, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Chat> Chats { get; set; }
    public virtual DbSet<GroupChat> GroupChats { get; set; }
    public virtual DbSet<Message> Messages { get; set; }
    public virtual DbSet<Friendship> FriendShips { get; set; }
    public virtual DbSet<FriendRequest> FriendRequests { get; set; }
    public virtual DbSet<GroupInvite> GroupInvites { get; set; }
    public virtual DbSet<Block> BlockList { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //specify chat relations since there EF can't infer them from the model because of diff types of users
        modelBuilder.Entity<GroupChat>()
            .HasMany(e => e.Moderators)
            .WithMany(e => e.ModeratedChats);
        modelBuilder.Entity<User>()
            .HasMany(u => u.Chats)
            .WithMany(c => c.Users);
        modelBuilder.Entity<GroupChat>()
            .HasOne(h => h.Owner)
            .WithMany(w => w.OwnedChats);

        //default behavior is cascade however all relationships except for chat messages need to be set to no action
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.NoAction;
        }

        //chat messages need to be deleted if chat is deleted
        modelBuilder.Entity<Chat>()
            .HasMany(h => h.Messages)
            .WithOne(w => w.Chat)
            .OnDelete(DeleteBehavior.Cascade);
        
        base.OnModelCreating(modelBuilder);
    }
}