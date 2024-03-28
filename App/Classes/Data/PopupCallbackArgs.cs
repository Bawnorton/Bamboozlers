using Blazorise;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Classes.Data;

public record PopupCallbackArgs(
    RenderFragment? BodyContent = null, 
    RenderFragment? HeaderContent = null, 
    RenderFragment? FooterContent = null, 
    ModalSize Size = ModalSize.Default
);