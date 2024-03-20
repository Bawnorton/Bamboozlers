using Bamboozlers.Classes.Events.Api;
using Bamboozlers.Classes.Interop;

namespace Bamboozlers.Classes.Events;

public abstract class MouseEvents
{
    public const string EventCssClass = "mouse-event-listener";

    public static readonly Event<MouseOverEvent> MouseOver = EventFactory.CreateArrayBacked<MouseOverEvent>(
        listeners => async (elementId, passed) =>
        {
            foreach (var listener in listeners)
            {
                await listener(elementId, passed);
            }
        });

    public static readonly Event<MouseOutEvent> MouseOut = EventFactory.CreateArrayBacked<MouseOutEvent>(
        listeners => async (elementId, passed) =>
        {
            foreach (var listener in listeners)
            {
                await listener(elementId, passed);
            }
        });

    public delegate Task MouseOverEvent(string elementId, bool passed);
    
    public delegate Task MouseOutEvent(string elementId, bool passed);
}