using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Services.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Provider;

public class MockUserService
{
    private readonly Mock<UserService> _mockUserService;
    private readonly MockUserManager _mockUserManager;
    private readonly MockAuthService _mockAuthService;
    
    public User? Self { get; set; }

    public MockUserService(TestContextBase ctx,
        User self,
        MockAuthService mockAuthService, 
        MockUserManager mockUserManager)
    {
        Self = self;
        _mockAuthService = mockAuthService;
        _mockUserManager = mockUserManager;
        
        _mockUserService = new Mock<UserService>(
            _mockAuthService.GetAuthService(),
            _mockUserManager.GetUserManager()
        );

        _mockUserService.Setup(x => x.Invalidate()).Callback(() =>
        {
            mockAuthService.GetAuthService().Invalidate();
        });
        
        _mockUserService.Setup(x => x.GetUserDataAsync()).ReturnsAsync(() =>
            Self is null 
                ? UserRecord.Default 
                : new UserRecord(
                    Self.Id,
                    Self.UserName,
                    Self.Email,
                    Self.DisplayName,
                    Self.Bio,
                    Self.Avatar
                )
        );
        
        ctx.Services.AddSingleton<IUserService>(_mockUserService.Object);
    }

    public IUserService GetUserService()
    {
        return _mockUserService.Object;
    }
}