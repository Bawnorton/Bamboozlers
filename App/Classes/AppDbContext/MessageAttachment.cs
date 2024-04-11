using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Bamboozlers.Classes.AppDbContext;

[PrimaryKey(nameof(MessageID), nameof(Index))]
[SuppressMessage("ReSharper", "EntityFramework.ModelValidation.UnlimitedStringLength")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MessageAttachment
{
    public MessageAttachment(int messageID)
    {
        MessageID = messageID;
    }
    public int MessageID { get; set; }
    public Message Message { get; set; } = default!;
    
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Index { get; set; }
    public string FileName { get; set; } = default!;
    public byte[] Data { get; set; } = default!;
}