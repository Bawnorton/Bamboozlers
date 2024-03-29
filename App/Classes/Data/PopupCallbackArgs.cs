using Blazorise;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Classes.Data;

public record PopupCallbackArgs(
    RecognizedPopupType PopupType = RecognizedPopupType.None,
    RenderFragment? BodyContent = null, 
    RenderFragment? HeaderContent = null, 
    RenderFragment? FooterContent = null, 
    ModalSize Size = ModalSize.Default
);

public enum RecognizedPopupType
{
    None,
    Settings,
    Profile
}