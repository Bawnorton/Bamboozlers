using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Bamboozlers.Classes.Services;

/*
 * HttpContext is only viable for static pages, not interactively rendered pages/components.
 * This class bridges a gap between static-rendered pages that can utilize HttpContext and dynamically rendered pages
 * that cannot do so.
 */
public class UserService
{
    private ClaimsPrincipal currentUser = new(new ClaimsIdentity());

    public ClaimsPrincipal GetUser()
    {
        return currentUser;
    }

    internal void SetUser(ClaimsPrincipal user)
    {
        currentUser = user ?? throw new ArgumentNullException(nameof(user));
    }
}

internal sealed class UserCircuitHandler : CircuitHandler, IDisposable
{
    private readonly AuthenticationStateProvider authenticationStateProvider;
    private readonly UserService userService;

    public UserCircuitHandler(AuthenticationStateProvider authenticationStateProvider, UserService userService)
    {
        this.authenticationStateProvider = authenticationStateProvider;
        this.userService = userService;
    }

    public void Dispose()
    {
        authenticationStateProvider.AuthenticationStateChanged -= AuthenticationChanged;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        authenticationStateProvider.AuthenticationStateChanged += AuthenticationChanged;

        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }

    private void AuthenticationChanged(Task<AuthenticationState> task)
    {
        _ = UpdateAuthentication(task);

        async Task UpdateAuthentication(Task<AuthenticationState> task)
        {
            try
            {
                var state = await task;
                userService.SetUser(state.User);
            }
            catch
            {
                // Exceptions can't be reported here, will be reported elsewhere in the application later
            }
        }
    }

    public override async Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        var state = await authenticationStateProvider.GetAuthenticationStateAsync();
        userService.SetUser(state.User);
    }
}