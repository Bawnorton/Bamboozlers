using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Services.UserServices;
using Bamboozlers.Classes.Utility.Observer;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components;

public class UserViewComponentBase : ComponentBase, IUserSubscriber
{
    [Inject] protected IUserService UserService { get; set; } = default!;
    [Inject] protected IAuthService AuthService { get; set; } = default!;
    public UserRecord? UserData { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        UserService.AddSubscriber(this);
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (UserData is null)
        {
            UserData = await UserService.GetUserDataAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    public virtual async Task OnUpdate(UserRecord? data)
    {
        UserData = data ?? UserData;
        await InvokeAsync(StateHasChanged);
    }

    void ISubscriber.OnUpdate()
    {
        InvokeAsync(StateHasChanged);
    }

    public async Task OnUpdate()
    {
        await InvokeAsync(StateHasChanged);
    }
}