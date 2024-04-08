using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Bamboozlers.Classes.AppDbContext;

[SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
public class MessageAttachment
{
    [Key] public int ID { get; set; }
    public string FileName { get; set; }
    public byte[] Data { get; set; }
}