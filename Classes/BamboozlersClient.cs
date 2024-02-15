using Bamboozlers.Classes.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes;

public class BamboozlersClient
{
    public static BamboozlersClient Instance { get; } = new();

    public AppDbContext.AppDbContext Db { get; private set; }

    public WindowManager WindowManager { get; } = new();
    
    private int _userId;

    /// <summary>
    /// Impl Note: In rendered contexts set via <see cref="Microsoft.AspNetCore.Components.ComponentBase.InvokeAsync(System.Action)"/>
    /// </summary>
    public int UserId
    {
        get => _userId;
        set
        {
            _userId = value;
            User = Db.Users
                .Include(user => user.Chats)
                .ThenInclude(chat => chat.Users)
                .FirstOrDefaultAsync(user => user.ID == value).Result;
            if (User is null)
            {
                throw new Exception("User not found for id " + value);
            }

            Chats = User.Chats.ToList().Select(chat => new ClientChat
            {
                DbChat = chat,
                Name = chat is GroupChat gc1 ? gc1.Name : chat.Users.First(user => user.ID != value).Username,
                Avatar = (chat is GroupChat gc2 ? gc2.Avatar : chat.Users.First(user => user.ID != value).Avatar) is null ? null : "https://via.placeholder.com/24"
            }).ToList();
            NotifyStateChanged();
        }
    }
    
    private User? User { get; set; }

    public List<ClientChat> Chats { get; private set; }

    private IEnumerable<ClientChat>? _directChatsCache;
    
    private IEnumerable<ClientChat>? _groupChatsCache;

    public IEnumerable<ClientChat> DirectChats => _directChatsCache ??= Chats.Where(chat => chat.IsDirectChat);

    public IEnumerable<ClientChat> GroupChats => _groupChatsCache ??= Chats.Where(chat => chat.IsGroupChat);
    
    public string Username => User?.Username ?? "Unknown";
    
    public event Action? OnChange;
    public void Init(AppDbContext.AppDbContext dbContext)
    {
        Db = dbContext;
    }
    private void NotifyStateChanged() => OnChange?.Invoke();
}