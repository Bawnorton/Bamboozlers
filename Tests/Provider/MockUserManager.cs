using System.Security.Claims;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Services.Authentication;
using Bunit.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Tests.Provider;

namespace Tests;

public class MockUserManager
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    
    private readonly MockDatabaseProvider _mockDatabaseProvider;

    private readonly AppDbContext _mockAppDbContext;
    
    public MockUserManager(TestContextBase ctx, MockDatabaseProvider mockDatabaseProvider)
    {
        /* Build test user data entries */
        _mockDatabaseProvider = mockDatabaseProvider;
        _mockAppDbContext = _mockDatabaseProvider.GetDbContextFactory().CreateDbContext();
        
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

        _mockUserManager.Setup(x 
                => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())
            ).ReturnsAsync((ClaimsPrincipal claimsPrincipal) 
                => claimsPrincipal.Identity is null ? null : _mockAppDbContext.Users.FirstOrDefault(u => u.UserName == claimsPrincipal.Identity.Name)
        );
        
        /* Methods that can/need to be tested */
        _mockUserManager.Setup(x 
            => x.FindByEmailAsync(It.IsAny<string>())
        ).ReturnsAsync((string? email) 
            => _mockAppDbContext.Users.FirstOrDefault(m => m.Email == email)
        );
        
        _mockUserManager.Setup(x 
            => x.FindByIdAsync(It.IsAny<string>())
        ).ReturnsAsync((string? userId) 
            => userId is not null ? _mockAppDbContext.Users.FirstOrDefault(m => m.Id == int.Parse(userId)) : null
        );
        
        _mockUserManager.Setup(x 
            => x.FindByNameAsync(It.IsAny<string>())
        ).ReturnsAsync((string? userName) 
            => _mockAppDbContext.Users.FirstOrDefault(m => m.UserName == userName)
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
            return _mockAppDbContext.Users.FirstOrDefault(m => m.Email == newEmail) is null 
                    ? IdentityResult.Success : 
                      IdentityResult.Failed([new IdentityError { Description = "Email is already in use." }]);
        });

        _mockUserManager.Setup(x
            => x.SetUserNameAsync(It.IsAny<User>(), It.IsAny<string>())
        ).ReturnsAsync((User user, string? newUsername)
            => {
            if (newUsername.IsNullOrEmpty())
                return IdentityResult.Failed([new IdentityError { Description = "Username entered was null or empty." }]);
            return _mockAppDbContext.Users.FirstOrDefault(m => m.UserName == newUsername) is null 
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
        if (idx == -1) idx = _mockAppDbContext.Users.Count();
        var newUser = new User
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
        
        _mockAppDbContext.Users.Add(newUser);
        return newUser;
    }

    public User? GetMockUser(int idx)
    {
        return _mockAppDbContext.Users.FirstOrDefault(u => u.Id == idx);
    }
    
    public void ClearMockUsers()
    {
        _mockAppDbContext.Users.RemoveRange(_mockAppDbContext.Users);
    }
    
    public UserManager<User> GetUserManager()
    {
        return _mockUserManager.Object;
    }

    public AppDbContext GetWorkingDbContext()
    {
        return _mockAppDbContext;
    }
}