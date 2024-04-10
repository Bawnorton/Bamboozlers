using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Bamboozlers.Classes.AppDbContext;

[SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MessageAttachment
{
    [Key] public int ID { get; set; } 
    public string FileName { get; set; } = default!;
    public byte[] Data { get; set; } = default!;
}