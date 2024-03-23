using Bamboozlers.Classes.Events.Api;

namespace Bamboozlers.Classes.Events;

public abstract class KeyboardEvents
{
    public const string EventCssClass = "keyboard-event-listener";
    
    public static readonly Event<KeydownEvent> Keydown = EventFactory.CreateArrayBacked<KeydownEvent>(listeners =>
        async (key, code, ctrl, shift, alt, meta) =>
        {
            foreach (var listener in listeners)
            {
                await listener(key, code, ctrl, shift, alt, meta);
            }
        });

    public static readonly Event<KeyupEvent> Keyup = EventFactory.CreateArrayBacked<KeyupEvent>(listeners =>
        async (key, code, ctrl, shift, alt, meta) =>
        {
            foreach (var listener in listeners)
            {
                await listener(key, code, ctrl, shift, alt, meta);
            }
        });

    public delegate Task KeydownEvent(string key, string code, bool ctrl, bool shift, bool alt, bool meta);

    public delegate Task KeyupEvent(string key, string code, bool ctrl, bool shift, bool alt, bool meta);
}