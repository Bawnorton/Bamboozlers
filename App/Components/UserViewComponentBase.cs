using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Services.UserServices;
using Bamboozlers.Classes.Utility.Observer;
using Bamboozlers.Pages;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components;

public class UserViewComponentBase : ComponentBase, IUserSubscriber, IAsyncSubscriber
{
    [Inject] protected IUserService UserService { get; set; }
    [Inject] protected IAuthService AuthService { get; set; } = default!;
    
    protected UserRecord? UserData { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        UserService.AddSubscriber(this);
    }
    
    public void OnUpdate(UserRecord? data)
    {
        UserData = data ?? UserData;
        StateHasChanged();
    }

    void ISubscriber.OnUpdate()
    {
        StateHasChanged();
    }

    public Task OnUpdate()
    {
        StateHasChanged();
        return Task.CompletedTask;
    }
}