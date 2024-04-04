using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Services;
using Bamboozlers.Classes.Services.UserServices;
using Bamboozlers.Classes.Utility.Mediator;
using Bamboozlers.Classes.Utility.Observer;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components;

public class UserViewComponentBase : ComponentBase, IUserSubscriber, IAsyncSubscriber, IPopupColleague
{
    [Inject] protected IUserService UserService { get; set; } = default!;
    [Inject] protected IAuthService AuthService { get; set; } = default!;
    [Inject] public IPopupService PopupService { get; set; } = default!;
    protected UserRecord? UserData { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        UserService.AddSubscriber(this);
        PopupService.RegisterColleague(this);
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