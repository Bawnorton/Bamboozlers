using Bamboozlers.Classes.Events;
using Bamboozlers.Classes.Interop;
using Microsoft.JSInterop;

namespace Bamboozlers.Classes.Services;

public class EventService : IEventService, IDisposable
{
    private DotNetObjectReference<EventService>? _reference;
    private bool _initialized;
    
    public async Task Register(IJSRuntime jsRuntime)
    {
        _reference = DotNetObjectReference.Create(this);
        if (!_initialized)
        {
            await Initialize(jsRuntime);
        }
        await jsRuntime.InvokeVoidAsync("keyboardInterop.register", _reference, KeyboardEvents.EventCssClass);
        await jsRuntime.InvokeVoidAsync("inputInterop.register", _reference, InputEvents.EventCssClass);
        await jsRuntime.InvokeVoidAsync("mouseInterop.register", _reference, MouseEvents.EventCssClass);
        await jsRuntime.InvokeVoidAsync("clipboardInterop.register", _reference, ClipboardEvents.EventCssClass);
    }

    public DotNetObjectReference<T> GetReference<T>() where T : class, IEventService
    {
        return (_reference as DotNetObjectReference<T>)!;
    }

    private async Task Initialize(IJSRuntime jsRuntime)
    {
        _initialized = true;
        await jsRuntime.InvokeVoidAsync("keyboardInterop.init", _reference);
    }

    [JSInvokable]
    public async Task OnKeydown(KeyData data)
    {
        await KeyboardEvents.Keydown.Invoker().Invoke(data.key, data.code, data.ctrl, data.shift, data.alt, data.meta);
    }
    
    [JSInvokable]
    public async Task OnKeyup(KeyData data)
    {
        await KeyboardEvents.Keyup.Invoker().Invoke(data.key, data.code, data.ctrl, data.shift, data.alt, data.meta);
    }

    [JSInvokable]
    public async Task<List<KeyData>> OnGetDisallowedInputs()
    {
        return await InputEvents.DisallowedInputs.Invoker().Invoke();
    }

    [JSInvokable]
    public async Task OnInputKeydown(InputData data)
    {
        await InputEvents.InputKeydown.Invoker().Invoke(data.elementId!, data.key, data.code, data.ctrl, data.shift, data.alt, data.meta, data.content, data.passed);
    }
    
    [JSInvokable]
    public async Task OnInputKeyup(InputData data)
    {
        await InputEvents.InputKeyup.Invoker().Invoke(data.elementId!, data.key, data.code, data.ctrl, data.shift, data.alt, data.meta, data.content, data.passed);
    }
    
    [JSInvokable]
    public async Task OnMouseOver(MouseData data)
    {
        await MouseEvents.MouseOver.Invoker().Invoke(data.elementId!, data.passed);
    }
    
    [JSInvokable]
    public async Task OnMouseOut(MouseData data)
    {
        await MouseEvents.MouseOut.Invoker().Invoke(data.elementId!, data.passed);
    }
    
    [JSInvokable]
    public async Task<string> OnCopy(ClipboardData data)
    {
        return await ClipboardEvents.OnCopy.Invoker().Invoke(data.elementId, data.text);
    }
    
    [JSInvokable]
    public async Task<string> OnCut(ClipboardData data)
    {
        return await ClipboardEvents.OnCut.Invoker().Invoke(data.elementId, data.text);
    }
    
    [JSInvokable]
    public async Task<string> OnPaste(ClipboardData data)
    {
        return await ClipboardEvents.OnPaste.Invoker().Invoke(data.elementId, data.text);
    }

    public void Dispose()
    {
        if (_reference is not null)
            _reference.Dispose();
        GC.SuppressFinalize(this);
    }
}


public interface IEventService
{
    /// <summary>
    /// Register the current component for receiving events from JsInterop
    /// **EVENTS RELATED TO JS INTEROP WILL NOT WORK WITHOUT THIS**
    /// </summary>
    Task Register(IJSRuntime jsRuntime);

    DotNetObjectReference<T> GetReference<T>() where T : class, IEventService;
}