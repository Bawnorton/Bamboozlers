using Bamboozlers.Classes.Events;
using Microsoft.JSInterop;

namespace Bamboozlers.Classes.Interop;

public abstract class JsInteropHelper
{
    private static IJSRuntime? _jsRuntime;
    
    public static void Init(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    
    public static async Task Register()
    {
        if (_jsRuntime is not null) {
            await _jsRuntime.InvokeVoidAsync("keyboardInterop.register", KeyboardEvents.EventCssClass);
            await _jsRuntime.InvokeVoidAsync("inputInterop.register", InputEvents.EventCssClass);
            await _jsRuntime.InvokeVoidAsync("mouseInterop.register", MouseEvents.EventCssClass);
            await _jsRuntime.InvokeVoidAsync("clipboardInterop.register", ClipboardEvents.EventCssClass);
        }
    }
    
    public static IJSRuntime GetJsRuntime()
    {
        return _jsRuntime;
    }

    [JSInvokable]
    public static async Task OnKeydown(KeyData data)
    {
        await KeyboardEvents.Keydown.Invoker().Invoke(data.key, data.code, data.ctrl, data.shift, data.alt, data.meta);
    }
    
    [JSInvokable]
    public static async Task OnKeyup(KeyData data)
    {
        await KeyboardEvents.Keyup.Invoker().Invoke(data.key, data.code, data.ctrl, data.shift, data.alt, data.meta);
    }

    [JSInvokable]
    public static async Task<List<KeyData>> OnGetDisallowedInputs()
    {
        return await InputEvents.DisallowedInputs.Invoker().Invoke();
    }

    [JSInvokable]
    public static async Task OnInputKeydown(InputData data)
    {
        await InputEvents.InputKeydown.Invoker().Invoke(data.elementId!, data.key, data.code, data.ctrl, data.shift, data.alt, data.meta, data.content, data.passed);
    }
    
    [JSInvokable]
    public static async Task OnInputKeyup(InputData data)
    {
        await InputEvents.InputKeyup.Invoker().Invoke(data.elementId!, data.key, data.code, data.ctrl, data.shift, data.alt, data.meta, data.content, data.passed);
    }
    
    [JSInvokable]
    public static async Task OnMouseOver(MouseData data)
    {
        await MouseEvents.MouseOver.Invoker().Invoke(data.elementId!, data.passed);
    }
    
    [JSInvokable]
    public static async Task OnMouseOut(MouseData data)
    {
        await MouseEvents.MouseOut.Invoker().Invoke(data.elementId!, data.passed);
    }
    
    [JSInvokable]
    public static async Task<string> OnCopy(ClipboardData data)
    {
        return await ClipboardEvents.OnCopy.Invoker().Invoke(data.elementId, data.text);
    }
    
    [JSInvokable]
    public static async Task<string> OnCut(ClipboardData data)
    {
        return await ClipboardEvents.OnCut.Invoker().Invoke(data.elementId, data.text);
    }
    
    [JSInvokable]
    public static async Task<string> OnPaste(ClipboardData data)
    {
        return await ClipboardEvents.OnPaste.Invoker().Invoke(data.elementId, data.text);
    }

}