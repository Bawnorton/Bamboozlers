using Bamboozlers.Classes.AppDbContext;
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
    
    protected User? Self { get; set; }
    
    private bool Initialized { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Initialized = true;
        await base.OnInitializedAsync();
        UserService.AddSubscriber(this);
        Self = await AuthService.GetUser();
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(!Initialized) throw new InvalidOperationException("Component not initialized, ensure base.OnInitializedAsync() is called");
        
        await base.OnAfterRenderAsync(firstRender);
        if (UserData is null)
        {
            UserData = await UserService.GetUserDataAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    public virtual async Task OnUpdate(UserRecord? data)
    {
        Self = (await AuthService.GetUser())!;
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