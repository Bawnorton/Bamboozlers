using System.Diagnostics;
using Bamboozlers.Classes.Services.UserService;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components;

public class UserViewComponentBase : ComponentBase
{
    [Inject] 
    public IUserService UserService { get; set; } = default!;

    protected override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();
    }
}