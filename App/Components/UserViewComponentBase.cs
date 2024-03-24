
using System.Diagnostics;
using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Services.Authentication;
using Bamboozlers.Classes.Utility.Observer;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components;

public class UserViewComponentBase : ComponentBase, ISubscriber
{
    [Inject] protected IUserService UserService { get; set; } = default!;

    [Inject] protected IAuthService AuthService { get; set; } = default!;

    public UserRecord? UserData { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        UserService.AddSubscriber(this);
        await OnUpdateAsync();
    }

    public virtual async Task OnUpdateAsync()
    {
        UserData = await UserService.GetUserDataAsync();
        StateHasChanged();
    }
}