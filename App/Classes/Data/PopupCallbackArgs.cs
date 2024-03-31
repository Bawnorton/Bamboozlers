using Bamboozlers.Classes.AppDbContext;
using Blazorise;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Classes.Data;

public record PopupCallbackArgs(
    RecognizedPopupType PopupType = RecognizedPopupType.None,
    RenderFragment? BodyContent = null, 
    RenderFragment? HeaderContent = null, 
    RenderFragment? FooterContent = null, 
    ModalSize Size = ModalSize.Default,
    User? FocusUser = null, // For pop-ups that focus on a User in specific
    int? ChatId = null // For pop-ups that focus on a Chat in specific
);

public enum RecognizedPopupType
{
    None,
    Settings,
    Profile,
    GroupChatSettings,
    ChatAddMember,
    
}