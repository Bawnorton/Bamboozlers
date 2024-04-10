using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Components.Chat;

public class FileHolder : ComponentBase
{
    [Parameter] public byte[] FileData { get; set; } = default!;
    [Parameter] public string FileName { get; set; } = default!;
    protected ImageTypes? ImageType { get; set; }
    
    protected bool IsImage => ImageType.HasValue;
    
    protected string FileSize => FileData.Length switch
    {
        < 1024 => $"{FileData.Length} B",
        < 1024 * 1024 => $"{FileData.Length / 1024} KB",
        < 1024 * 1024 * 1024 => $"{FileData.Length / 1024 / 1024} MB",
        _ => $"{FileData.Length / 1024 / 1024 / 1024} GB"
    };

    protected override async Task OnParametersSetAsync()
    {
        var imageType = await GetImageType(FileData);
        ImageType = imageType;
    }
    
    protected static Task<ImageTypes?> GetImageType(byte[] fileData)
    { 
        if (fileData.Length == 0) return Task.FromResult<ImageTypes?>(null);

        var jpg = new byte[] { 0xFF, 0xD8 };
        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
        var webp = new byte[] { 0x52, 0x49, 0x46, 0x46 };
        var bmp = new byte[] { 0x42, 0x4D };

        if (fileData.Take(jpg.Length).SequenceEqual(jpg))
        {
            return Task.FromResult<ImageTypes?>(ImageTypes.Jpg);
        }

        if (fileData.Take(png.Length).SequenceEqual(png))
        {
            return Task.FromResult<ImageTypes?>(ImageTypes.Png);
        }

        if (fileData.Take(webp.Length).SequenceEqual(webp))
        {
            return Task.FromResult<ImageTypes?>(ImageTypes.Webp);
        }

        if (fileData.Take(bmp.Length).SequenceEqual(bmp))
        {
            return Task.FromResult<ImageTypes?>(ImageTypes.Bmp);
        }

        return Task.FromResult<ImageTypes?>(null);
    }

    protected enum ImageTypes
    {
        Jpg,
        Png,
        Webp,
        Bmp
    }
}