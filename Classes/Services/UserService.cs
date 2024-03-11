using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Data.ViewModel;
using Bamboozlers.Migrations;
using Microsoft.AspNetCore.Identity;

namespace Bamboozlers.Classes.Services;

public static class UserService
{
    private static UserManager<User> _userManager;

    public static void Init(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public static async Task<User?> GetUserAsync()
    {
        return await _userManager.FindByIdAsync((await GetUserId()).ToString());
    }
    
    public static async Task<int> GetUserId()
    {
        var user = await AuthHelper.GetSelf();
        return user?.Id ?? -1;
    }

    public static async Task UpdateUserAsync(User user)
    {
        await _userManager.UpdateAsync(user); 
        await _userManager.UpdateSecurityStampAsync(user);
        
        UserDisplayRecord.Update(user);
    }
}