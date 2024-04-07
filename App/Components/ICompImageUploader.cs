using Bamboozlers.Classes.Data;
using Blazorise;
using Blazorise.Extensions;
using Microsoft.AspNetCore.Components.Forms;

namespace Bamboozlers.Components;

public interface ICompImageUploader
{
    private static readonly string[] AllowedFormats = ["png", "jpeg", "jpeg"];

    public async Task<AlertArguments> OnFileUpload(InputFileChangeEventArgs args, Func<byte[], Task<bool>> setCallback)
    {
        IBrowserFile? upload;
        try
        {
            upload = args.File;
        }
        catch (InvalidOperationException)
        {
            return new AlertArguments(
                Color.Danger,
                true,
                "Error occured while uploading image.",
                "No file was uploaded."
            );
        }

        if (!upload.ContentType.Contains("image/"))
            return new AlertArguments(
                Color.Danger,
                true,
                "Error occured while uploading image.",
                "Uploaded file was not an image."
            );

        var imageType = upload.ContentType[6..];
        if (!AllowedFormats.Contains(imageType))
            return new AlertArguments(
                Color.Danger,
                true,
                "Error occured while uploading image.",
                "Image must be a PNG or JPG (JPEG) file."
            );

        ArraySegment<byte> bytes;
        bool success;
        using (var stream = new MemoryStream())
        {
            try
            {
                await upload.OpenReadStream().CopyToAsync(stream);
                success = stream.TryGetBuffer(out bytes);
            }
            catch (Exception)
            {
                return new AlertArguments(
                    Color.Danger,
                    true,
                    "Error occured while uploading image.",
                    "Unknown error occurred. Please try again."
                );
            }
        }

        var image = bytes.ToArray();
        if (!success || image.IsNullOrEmpty())
            return new AlertArguments(
                Color.Danger,
                true,
                "Error occured while uploading image.",
                "Unknown error occurred. Please try again."
            );
        try
        {
            var result = await setCallback.Invoke(image.ToArray());
            if (result)
                return new AlertArguments(
                    Color.Success,
                    true,
                    "Success!",
                    "Image was successfully uploaded."
                );
            throw new Exception();
        }
        catch (Exception)
        {
            return new AlertArguments(
                Color.Danger,
                true,
                "Error occured while uploading image.",
                "Unknown error occurred. Please try again."
            );
        }
    }
}