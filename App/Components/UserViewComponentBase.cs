using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Services.UserServices;
using Bamboozlers.Classes.Utility.Observer;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components;

public class UserViewComponentBase : ComponentBase, IAsyncUserSubscriber
{
    [Inject] protected IUserService UserService { get; set; }
    [Inject] protected IAuthService AuthService { get; set; } = default!;
    protected UserRecord? UserData { get; set; }

    public virtual async Task OnUserUpdate()
    {
        UserData = await UserService.GetUserDataAsync();
        StateHasChanged();
    }

    public Task OnUpdate()
    {
        StateHasChanged();
        return Task.CompletedTask;
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        UserService.AddSubscriber(this);
    }
}