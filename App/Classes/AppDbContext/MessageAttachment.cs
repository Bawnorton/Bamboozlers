using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.AppDbContext;

[PrimaryKey(nameof(ID), nameof(Index))]
[SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MessageAttachment
{
    public int ID { get; set; }
    public int Index { get; set; }
    public string FileName { get; set; } = default!;
    public byte[] Data { get; set; } = default!;
}