
using System.Diagnostics;
using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Services.UserService;
using Bamboozlers.Classes.Utility.Observer;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components;

public class UserViewComponentBase : ComponentBase, ISubscriber
{
    [Inject] 
    protected IUserService UserService { get; set; } = default!;

    protected UserRecord? UserData { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await UserService.Initialize();
        UserService.AddSubscriber(this);
        OnUpdate();
    }

    public virtual void OnUpdate()
    {
        Debug.WriteLine("RECEIVED SIGNAL");
        UserData = UserService.GetUserData();
        StateHasChanged();
    }
}