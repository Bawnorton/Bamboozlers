using Bamboozlers.Classes.AppDbContext;
using Bunit.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;

namespace Tests;

public class MockUserManager
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    
    private readonly Mock<IDbContextFactory<AppDbContext>> _mockDbContextFactory;
    
    private readonly List<User> _mockUsers = [];
    
    public MockUserManager(TestContextBase ctx)
    {
        // Initialize a mock backing database
        _mockDbContextFactory = new Mock<IDbContextFactory<AppDbContext>>();
        var options = new DbContextOptions<AppDbContext>();
        var mockDbContext = new Mock<AppDbContext>(options);
        
        _mockDbContextFactory.Setup(x => x.CreateDbContext()).Returns(mockDbContext.Object);
        _mockDbContextFactory.Setup(x => x.CreateDbContextAsync(default)).ReturnsAsync(mockDbContext.Object);
        
        /* Build test user data entries */
        mockDbContext.Setup(x => x.Users).Returns(_mockUsers.AsQueryable().BuildMockDbSet().Object);
        
        /* Build false data sets */
        mockDbContext.Setup(x => x.Chats).Returns(new List<Chat>().AsQueryable().BuildMockDbSet().Object);
        mockDbContext.Setup(x => x.BlockList).Returns(new List<Block>().AsQueryable().BuildMockDbSet().Object);
        mockDbContext.Setup(x => x.FriendRequests).Returns(new List<FriendRequest>().AsQueryable().BuildMockDbSet().Object);
        mockDbContext.Setup(x => x.FriendShips).Returns(new List<Friendship>().AsQueryable().BuildMockDbSet().Object);
        mockDbContext.Setup(x => x.GroupInvites).Returns(new List<GroupInvite>().AsQueryable().BuildMockDbSet().Object);
        
        ctx.Services.AddSingleton(_mockDbContextFactory.Object);
        
        _mockUserManager = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), 
            Mock.Of<IOptions<IdentityOptions>>(), 
            Mock.Of<IPasswordHasher<User>>(), 
            null, // No need for user validator
            null, // No need for password validator
            Mock.Of<ILookupNormalizer>(), 
            Mock.Of<IdentityErrorDescriber>(), 
            Mock.Of<IServiceProvider>(), 
            null
        );
        
        ctx.Services.AddSingleton(_mockUserManager.Object);

        /* Leave the testing of these methods to Microsoft */
        _mockUserManager.Setup(x
            => x.CreateAsync(It.IsAny<User>()) 
        ).ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(x
            => x.DeleteAsync(It.IsAny<User>())
        ).ReturnsAsync(IdentityResult.Success);
        
        _mockUserManager.Setup(x 
            => x.UpdateAsync(It.IsAny<User>())
        ).ReturnsAsync(IdentityResult.Success);
        
        _mockUserManager.Setup(x 
            => x.UpdateSecurityStampAsync(It.IsAny<User>())
        ).ReturnsAsync(IdentityResult.Success);

        /* Methods that can/need to be tested */
        _mockUserManager.Setup(x 
            => x.FindByEmailAsync(It.IsAny<string>())
        ).ReturnsAsync((string? email) 
            => _mockUsers.FirstOrDefault(m => m.Email == email)
        );
        
        _mockUserManager.Setup(x 
            => x.FindByIdAsync(It.IsAny<string>())
        ).ReturnsAsync((string? userId) 
            => userId is not null ? _mockUsers.FirstOrDefault(m => m.Id == int.Parse(userId)) : null
        );
        
        _mockUserManager.Setup(x 
            => x.FindByNameAsync(It.IsAny<string>())
        ).ReturnsAsync((string? userName) 
            => _mockUsers.FirstOrDefault(m => m.UserName == userName)
        );

        _mockUserManager.Setup(x 
            => x.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>())
        ).ReturnsAsync((User user, string? password) => password is not null && password.Equals(user.PasswordHash));

        _mockUserManager.Setup(x
            => x.ChangePasswordAsync(It.IsAny<User>(),It.IsAny<string>(),It.IsAny<string>())
        ).ReturnsAsync((User user, string? password, string? newPassword) 
            => {
            if (password.IsNullOrEmpty())
                return IdentityResult.Failed([new IdentityError{Description="Password entered was null or empty."}]);
            return password.Equals(user.PasswordHash) ? IdentityResult.Success : IdentityResult.Failed([new IdentityError{Description="Password does not match."}]);
        });
        
        _mockUserManager.Setup(x 
            => x.ChangeEmailAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>())
        ).ReturnsAsync((User user, string? newEmail, string? token) 
            => {
            if (newEmail.IsNullOrEmpty())
                return IdentityResult.Failed([new IdentityError { Description = "Email entered was null or empty." }]);
            return _mockUsers.FirstOrDefault(m => m.Email == newEmail) is null 
                    ? IdentityResult.Success : 
                      IdentityResult.Failed([new IdentityError { Description = "Email is already in use." }]);
        });

        _mockUserManager.Setup(x
            => x.SetUserNameAsync(It.IsAny<User>(), It.IsAny<string>())
        ).ReturnsAsync((User user, string? newUsername)
            => {
            if (newUsername.IsNullOrEmpty())
                return IdentityResult.Failed([new IdentityError { Description = "Username entered was null or empty." }]);
            return _mockUsers.FirstOrDefault(m => m.UserName == newUsername) is null 
                ? IdentityResult.Success : 
                  IdentityResult.Failed([new IdentityError { Description = "Username is already in use." }]);
        });
    }

    public User CreateMockUser(
        int idx = -1, 
        bool emailConfirmed = true, 
        string? displayName = null, 
        string? description = null, 
        byte[]? avatar = null)
    {
        if (idx == -1) idx = _mockUsers.Count;
        var tuser = new User
        {
            Id = idx,
            UserName = $"TestUser{idx}",
            Email = $"test.user{idx}@gmail.com",
            EmailConfirmed = emailConfirmed,
            PasswordHash = $"@Password{idx}",
            DisplayName = displayName,
            Bio = description,
            Avatar = avatar
        };
        
        _mockUsers.Add(tuser);
        return tuser;
    }

    public User GetMockUser(int idx)
    {
        return _mockUsers.Find(u => u.Id == idx) ?? CreateMockUser(idx);
    }

    public void ClearMockUsers()
    { 
        _mockUsers.Clear();    
    }
    
    public UserManager<User> GetUserManager()
    {
        return _mockUserManager.Object;
    }

    public IDbContextFactory<AppDbContext> GetDbContextFactory()
    {
        return _mockDbContextFactory.Object;
    }
}