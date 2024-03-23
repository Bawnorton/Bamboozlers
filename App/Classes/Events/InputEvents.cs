using Bamboozlers.Classes.Events.Api;
using Bamboozlers.Classes.Interop;

namespace Bamboozlers.Classes.Events;

public abstract class InputEvents
{
    public const string EventCssClass = "input-event-listener";
    
    public static readonly Event<DisallowedInputsEvent> DisallowedInputs =
        EventFactory.CreateArrayBacked<DisallowedInputsEvent>(listeners =>
            async () =>
            {
                var disallowedKeys = new List<KeyData>();
                foreach (var listener in listeners)
                {
                    disallowedKeys.AddRange(await listener());
                }

                return disallowedKeys;
            });

    public static readonly Event<InputKeydownEvent> InputKeydown = EventFactory.CreateArrayBacked<InputKeydownEvent>(
        listeners => async (elementId, key, code, ctrl, shift, alt, meta, content, passed) =>
            {
                foreach (var listener in listeners)
                {
                    await listener(elementId, key, code, ctrl, shift, alt, meta, content, passed);
                }
            });

    public static readonly Event<InputKeyupEvent> InputKeyup = EventFactory.CreateArrayBacked<InputKeyupEvent>(
        listeners => async (elementId, key, code, ctrl, shift, alt, meta, content, passed) =>
            {
                foreach (var listener in listeners)
                {
                    await listener(elementId, key, code, ctrl, shift, alt, meta, content, passed);
                }
            });


    public delegate Task<List<KeyData>> DisallowedInputsEvent();

    public delegate Task InputKeydownEvent(string elementId, string key, string code, bool ctrl, bool shift, bool alt, bool meta,
        string content, bool passed);

    public delegate Task InputKeyupEvent(string elementId, string key, string code, bool ctrl, bool shift, bool alt, bool meta,
        string content, bool passed);
}