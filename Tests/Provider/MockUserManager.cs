
using Bamboozlers.Classes;
using Bamboozlers.Classes.AppDbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;

namespace Tests;

public class MockUserManager
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly IUserStore<User> _mockUserStore;

    private readonly List<User> MockUsers = [];
    
    public MockUserManager(TestContextBase ctx)
    {
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
            => MockUsers.FirstOrDefault(m => m.Email == email)
        );
        
        _mockUserManager.Setup(x 
            => x.FindByIdAsync(It.IsAny<string>())
        ).ReturnsAsync((string? userId) 
            => userId is not null ? MockUsers.FirstOrDefault(m => m.Id == int.Parse(userId)) : null
        );
        
        _mockUserManager.Setup(x 
            => x.FindByNameAsync(It.IsAny<string>())
        ).ReturnsAsync((string? userName) 
            => MockUsers.FirstOrDefault(m => m.UserName == userName)
        );

        _mockUserManager.Setup(x 
            => x.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>())
        ).ReturnsAsync((User user, string? password) => CheckPassword(user, password));

        _mockUserManager.Setup(x
            => x.ChangePasswordAsync(It.IsAny<User>(),It.IsAny<string>(),It.IsAny<string>())
        ).ReturnsAsync((User user, string? password, string? newPassword) 
            => CheckPassword(user,password) ? IdentityResult.Success : IdentityResult.Failed()
        );
        
        _mockUserManager.Setup(x 
            => x.ChangeEmailAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>())
        ).ReturnsAsync((User user, string? newEmail, string? token) 
            => newEmail is not null && MockUsers.FirstOrDefault(m => m.Email == newEmail) is null ? IdentityResult.Success : IdentityResult.Failed()
        );

        _mockUserManager.Setup(x
            => x.SetUserNameAsync(It.IsAny<User>(), It.IsAny<string>())
        ).ReturnsAsync((User user, string? newUsername)
            => newUsername is not null && MockUsers.FirstOrDefault(m => m.UserName == newUsername)  is null ? IdentityResult.Success : IdentityResult.Failed()
        );
    }

    public User CreateMockUser(int idx = -1, 
        bool emailConfirmed = true, 
        string? displayName = null, 
        string? description = null, 
        byte[]? avatar = null)
    {
        if (idx == -1) idx = MockUsers.Count;
        User tuser = new User
        {
            Id = idx,
            UserName = $"TestUser{idx}",
            Email = $"test.user{idx}@gmail.com",
            EmailConfirmed = emailConfirmed,
        };
        // Avoid
        tuser.PasswordHash =  $"@Password{idx}";
        tuser.DisplayName ??= displayName;
        tuser.Bio ??= description;
        tuser.Avatar ??= avatar;
        
        MockUsers.Add(tuser);
        return tuser;
    }

    public User GetMockUser(int idx)
    {
        return MockUsers.Find(u => u.Id == idx) ?? CreateMockUser(idx);
    }

    public void ClearMockUsers()
    {
        MockUsers.Clear();    
    }
    
    private bool CheckPassword(User user, string? password)
    {
        return password is not null && password.Equals(user.PasswordHash);
    }
    
    public UserManager<User> GetUserManager()
    {
        return _mockUserManager.Object;
    }
}