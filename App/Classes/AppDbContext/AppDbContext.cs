using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.AppDbContext;

public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Chat> Chats { get; set; }
    public virtual DbSet<ChatUser> ChatUsers { get; set; }
    public virtual DbSet<GroupChat> GroupChats { get; set; }
    public virtual DbSet<ChatModerator> ChatModerators { get; set; }
    public virtual DbSet<Message> Messages { get; set; }
    public virtual DbSet<Friendship> FriendShips { get; set; }
    public virtual DbSet<FriendRequest> FriendRequests { get; set; }
    public virtual DbSet<GroupInvite> GroupInvites { get; set; }
    public virtual DbSet<Block> BlockList { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        /*
         * Following tables require DeleteBehavior.NoAction:
         * Friendship
         * Block
         * GroupInvite
         * FriendRequest
         *
         * As these tables include references to two User instances, EF can't figure it out on its own.
         * These data points need to be manually deleted when a User is deleted.
         */
        foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            fk.DeleteBehavior = DeleteBehavior.NoAction;
        }

        modelBuilder.Entity<GroupInvite>()
            .HasOne(e => e.Group)
            .WithMany()
            .HasForeignKey(e => e.GroupID)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<GroupChat>()
            .HasMany(e => e.Moderators)
            .WithMany(e => e.ModeratedChats)
            .UsingEntity<ChatModerator>(
                l => l.HasOne<User>(e => e.User)
                    .WithMany(e => e.UserModeratedChats)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade),
                r => r.HasOne<GroupChat>(e => e.GroupChat)
                    .WithMany(e => e.ChatModeratorUsers)
                    .HasForeignKey(e => e.GroupChatId)
                    .OnDelete(DeleteBehavior.NoAction)
            );
        
        modelBuilder.Entity<User>()
            .HasMany(u => u.Chats)
            .WithMany(c => c.Users)
            .UsingEntity<ChatUser>(
                l => l.HasOne<Chat>(e => e.Chat)
                    .WithMany(e => e.ChatUsers)
                    .HasForeignKey(e => e.ChatId)
                    .OnDelete(DeleteBehavior.NoAction),
                r => r.HasOne<User>(e => e.User)
                    .WithMany(e => e.UserChats)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.Property(e => e.LastAccess).HasDefaultValueSql("CURRENT_TIMESTAMP");
                    j.Property(e => e.JoinDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                }
            );
        
        modelBuilder.Entity<GroupChat>()
            .HasOne(h => h.Owner)
            .WithMany(w => w.OwnedChats)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<Message>()
            .HasMany(m => m.Attachments)
            .WithOne()
            .HasForeignKey(a => a.ID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Message>()
            .HasOne<User>(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderID)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<Chat>()
            .HasMany(h => h.Messages)
            .WithOne(w => w.Chat)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}