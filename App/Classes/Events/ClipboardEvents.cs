using Bamboozlers.Classes.Events.Api;

namespace Bamboozlers.Classes.Events;

public abstract class ClipboardEvents
{
    public const string EventCssClass = "clipboard-event-listener";
    
    public static readonly Event<OnCopyEvent> OnCopy = EventFactory.CreateArrayBacked<OnCopyEvent>(
        listeners => async (elementId, text) =>
        {
            foreach (var listener in listeners)
            {
                text = await listener(elementId, text);
            }
            
            return text;
        });
    
    public static readonly Event<OnCutEvent> OnCut = EventFactory.CreateArrayBacked<OnCutEvent>(
        listeners => async (elementId, text) =>
        {
            foreach (var listener in listeners)
            {
                text = await listener(elementId, text);
            }

            return text;
        });
    
    public static readonly Event<OnPasteEvent> OnPaste = EventFactory.CreateArrayBacked<OnPasteEvent>(
        listeners => async (elementId, text) =>
        {
            foreach (var listener in listeners)
            {
                text = await listener(elementId, text);
            }

            return text;
        });

    public delegate Task<string> OnCopyEvent(string elementId, string text);
    
    public delegate Task<string> OnCutEvent(string elementId, string text);

    public delegate Task<string> OnPasteEvent(string elementId, string text);
}