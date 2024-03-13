using Bamboozlers.Classes.Events;
using Bamboozlers.Classes.Interop;
using Microsoft.JSInterop;

namespace Bamboozlers.Classes.Service;

public class EventService : IEventService, IDisposable
{
    private DotNetObjectReference<EventService> _reference;
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
    public async Task<List<InputData>> OnGetDisallowedInputs()
    {
        return await InputEvents.DisallowedInputs.Invoker().Invoke();
    }

    [JSInvokable]
    public async Task OnInputKeydown(InputData data)
    {
        await InputEvents.InputKeydown.Invoker().Invoke(data.key, data.code, data.ctrl, data.shift, data.alt, data.meta, data.content, data.passed);
    }
    
    [JSInvokable]
    public async Task OnInputKeyup(InputData data)
    {
        await InputEvents.InputKeyup.Invoker().Invoke(data.key, data.code, data.ctrl, data.shift, data.alt, data.meta, data.content, data.passed);
    }

    public void Dispose()
    {
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
    Task OnKeydown(KeyData data);
    Task OnKeyup(KeyData data);
    Task<List<InputData>> OnGetDisallowedInputs();
    Task OnInputKeydown(InputData data);
    Task OnInputKeyup(InputData data);
}