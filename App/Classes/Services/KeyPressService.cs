using Bamboozlers.Classes.Interop;
using Microsoft.JSInterop;

namespace Bamboozlers.Classes.Services;

public class KeyPressService : IKeyPressService
{
    private readonly DotNetObjectReference<KeyPressService> _dotNetObjectReference;

    private readonly IJSRuntime _jsRuntime;
    private bool _listening;

    public KeyPressService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _dotNetObjectReference = DotNetObjectReference.Create(this);
    }

    public event EventHandler<KeyEventArgs> KeyPressed = default!;
    public event EventHandler<KeyEventArgs> KeyReleased = default!;

    public async Task StartListeningAsync()
    {
        if (!_listening)
        {
            await _jsRuntime.InvokeVoidAsync("keypress.startListening", _dotNetObjectReference);
            _listening = true;
        }
    }

    public async Task StopListeningAsync()
    {
        if (_listening)
        {
            await _jsRuntime.InvokeVoidAsync("keypress.stopListening", _dotNetObjectReference);
            _listening = false;
        }
    }

    [JSInvokable("OnKeyPressed")]
    public void OnKeyPressed(KeyData keyData)
    {
        KeyPressed.Invoke(this, KeyEventArgs.FromKeyData(keyData));
    }

    [JSInvokable("OnKeyReleased")]
    public void OnKeyReleased(KeyData keyData)
    {
        KeyReleased.Invoke(this, KeyEventArgs.FromKeyData(keyData));
    }
}

public class KeyEventArgs(string key, string code, bool ctrlKey, bool shiftKey, bool altKey, bool metaKey) : EventArgs
{
    public string Key { get; } = key;
    public string Code { get; } = code;
    public bool CtrlKey { get; } = ctrlKey;
    public bool ShiftKey { get; } = shiftKey;
    public bool AltKey { get; } = altKey;
    public bool MetaKey { get; } = metaKey;

    public static KeyEventArgs FromKeyData(KeyData keyData)
    {
        return new KeyEventArgs(keyData.key, keyData.code, keyData.ctrl, keyData.shift, keyData.alt, keyData.meta);
    }
}

public interface IKeyPressService
{
    event EventHandler<KeyEventArgs> KeyPressed;
    event EventHandler<KeyEventArgs> KeyReleased;
    Task StartListeningAsync();
    Task StopListeningAsync();
}