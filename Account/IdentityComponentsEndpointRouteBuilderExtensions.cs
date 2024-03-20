using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Web;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Data.ViewModel;
using Bamboozlers.Classes.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Bamboozlers.Account;

public static class IdentityComponentsEndpointRouteBuilderExtensions
{
    // These endpoints are required by the Identity Razor components defined in the /Components/Account/Pages directory of this project.
    public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var accountGroup = endpoints.MapGroup("/Account");
        
        accountGroup.MapPost("/Logout", async (
            ClaimsPrincipal user,
            SignInManager<User> signInManager) =>
        {
            AuthHelper.Invalidate();
            await signInManager.SignOutAsync();
            return TypedResults.LocalRedirect($"~/Account/Login");
        });
        
        accountGroup.MapPost("/DeAuth", async (
            ClaimsPrincipal user,
            SignInManager<User> signInManager) =>
        {
            var builder = new UriBuilder();
            var urlParams = HttpUtility.ParseQueryString(string.Empty);
            urlParams["errorMessage"] = "Your account has been deleted.";
            builder.Query = urlParams.ToString();
            var callbackUrl = "~/Account/Login" + builder.Query;
            
            AuthHelper.Invalidate();
            await signInManager.SignOutAsync();
            return TypedResults.LocalRedirect(callbackUrl);
        });
        
        accountGroup.MapPost("/ReAuth", async (
            ClaimsPrincipal user,
            SignInManager<User> signInManager,
            [FromServices] UserManager<User> userManager) =>
        {
            await signInManager.SignOutAsync();
            
            var u = await userManager.GetUserAsync(user);
            
            if (u is null)
            {
                var builder = new UriBuilder();
                var urlParams = HttpUtility.ParseQueryString(string.Empty);
                urlParams["errorMessage"] = "Could not automatically sign you in after changing account details. Please log back in.";
                builder.Query = urlParams.ToString();
                
                var callbackUrl = "~/Account/Login" + builder.Query;
                
                return TypedResults.LocalRedirect(callbackUrl);
            }
            
            AuthHelper.Invalidate();
            await signInManager.SignInAsync(u, false);
            return TypedResults.LocalRedirect("~/");
        });

        /*
         * Can't get NavigationManager in here easily, so did a sort of workaround to make sure that the confirmation
         * email can be sent from a component in Interactive Server rendering.
         *
         * Could use some security improvements, but it might not be necessary for this project particularly.
         */
        accountGroup.MapPost("/SendConfirmationEmail", async (
            HttpContext context,
            [FromServices] UserManager<User> userManager,
            [FromServices] IEmailSender<User> emailSender,
            [FromBody] UserDataRecord parameters) =>
        {
            if (parameters.Id is null || parameters.Email is null) return;

            var userId = parameters.Id.ToString();
            var user = await userManager.FindByIdAsync(userId!);
            if (user is null) return;
            
            var code = await userManager.GenerateChangeEmailTokenAsync(user, parameters.Email);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            // TODO: This is probably not the most stellar way to perform this action, but it works for now. Could be improved.
            
            var urlParams = HttpUtility.ParseQueryString(string.Empty);
            urlParams["userId"] = parameters.Id.ToString();
            urlParams["email"] = parameters.Email;
            urlParams["code"] = code;
            
            var request = context.Request;
            var builder = new UriBuilder
            {
                Host = request.Host.Host,
                Scheme = request.Scheme,
                Port = request.Host.Port ?? 80,
                Path = "Account/ConfirmEmailChange",
                Query = urlParams.ToString()
            };

            var callbackUrl = builder.Uri.AbsoluteUri;
            
            await emailSender.SendConfirmationLinkAsync(
                user, 
                parameters.Email, 
                HtmlEncoder.Default.Encode(callbackUrl)
            );
        });
        
        var loggerFactory = endpoints.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var downloadLogger = loggerFactory.CreateLogger("DownloadPersonalData");
        
        accountGroup.MapPost("/DownloadPersonalData", async (
            HttpContext context,
            [FromServices] UserManager<User> userManager,
            [FromServices] AuthenticationStateProvider authenticationStateProvider) =>
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user is null)
            {
                return Results.NotFound($"Unable to load user with ID '{userManager.GetUserId(context.User)}'.");
            }

            var userId = await userManager.GetUserIdAsync(user);
            downloadLogger.LogInformation("User with ID '{UserId}' asked for their personal data.", userId);

            // Only include personal data for download
            var personalData = new Dictionary<string, string>();
            var personalDataProps = typeof(User).GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            foreach (var p in personalDataProps)
            {
                personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
            }

            var fileBytes = JsonSerializer.SerializeToUtf8Bytes(personalData);

            context.Response.Headers.TryAdd("Content-Disposition", "attachment; filename=PersonalData.json");
            return TypedResults.File(fileBytes, contentType: "application/json", fileDownloadName: "PersonalData.json");
        });

        return accountGroup;
    }
}