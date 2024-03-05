using Bamboozlers.Classes.AppDbContext;
using Blazorise;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components.Settings;

public partial class CompAvatar : CompProfile
{
    private async Task OnFileUpload(FileChangedEventArgs e)
    {
        var user = await GetUser();
        if (e.Files == null)
        {
            UpdateStatusArgs(new StatusArguments(
                Color.Danger,
                true,
                "Unable to change avatar.",
                "No file was uploaded."
            ));
            return;
        }
        MemoryStream result;
        try
        {
            using (result = new MemoryStream())
            {
                try
                {
                    await e.Files.First().OpenReadStream(long.MaxValue).CopyToAsync(result);
                }
                catch (InvalidOperationException)
                {
                    // Ignore this: there's one file, it returns the stream properly, but throws an exception unnecessarily
                }
            }
            var rawImage = new ArraySegment<byte>();
            
            var success = result.TryGetBuffer(out rawImage);
            if (!success)
            {
                UpdateStatusArgs(new StatusArguments(
                    Color.Danger,
                    true,
                    "Could not change avatar.",
                    "An error was encountered while processing uploaded avatar."
                    ));
                return;
            }
            /* I'm assuming we're going to be changing to base64 strings at some point, makes it easier to use in image elements
             
            string b64 = Convert.ToBase64String(result.GetBuffer());
            User.Avatar = b64;
             */
            user.Avatar = rawImage.ToArray();
            await DataChangeCallback.InvokeAsync();
        }
        catch (Exception exc)
        {
            Logger.LogError(exc.ToString());
            UpdateStatusArgs(new StatusArguments(
                Color.Danger,
                true,
                "Could not change avatar.",
                "An error was encountered while changing avatar."
                ));
        }
        finally
        {
            StateHasChanged();
        }
    }

    // TODO: Remove this once database has Base64 strings for avatar
    
    private static string GetBase64Avatar()
    {
        //return $"data:image/png;base64,{(user.Avatar == null ? "" : Convert.ToBase64String(user.Avatar))}";
        return "";
    }
}