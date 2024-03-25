
using System.Diagnostics;
using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Services.Authentication;
using Bamboozlers.Classes.Utility.Observer;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components;

public class UserViewComponentBase : ComponentBase, IAsyncSubscriber
{
    [Inject] protected IUserService UserService { get; set; } = default!;

    [Inject] protected IAuthService AuthService { get; set; } = default!;
    protected UserRecord? UserData { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        UserService.AddSubscriber(this);
    }
    
    public virtual async Task OnUpdate()
    {
        UserData = await UserService.GetUserDataAsync();
        StateHasChanged();
    }
}