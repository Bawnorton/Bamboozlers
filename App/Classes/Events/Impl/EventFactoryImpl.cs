using Bamboozlers.Classes.Events.Api;

namespace Bamboozlers.Classes.Events.Impl;

public abstract class EventFactoryImpl
{
    private static readonly HashSet<IArrayBackedEvent> ArrayBackedEvents = [];
    
    private EventFactoryImpl() { }
    
    public static void Invalidate()
    {
        foreach (var arrayBackedEvent in ArrayBackedEvents)
        {
            arrayBackedEvent.Update();
        }
    }

    public static Event<T> CreateArrayBacked<T>(Func<T[], T> invokerFactory)
    {
        var arrayBackedEvent = new ArrayBackedEvent<T>(invokerFactory);
        ArrayBackedEvents.Add(arrayBackedEvent);
        return arrayBackedEvent;
    }
    
    public static void EnsureContainsDefault(IEnumerable<string> phases)
    {
        if (phases.Any(phase => phase == Event<string>.DefaultPhase))
        {
            return;
        }

        throw new ArgumentException("Phases must contain the default phase");
    }

    public static void EnsureNoDuplicates(IEnumerable<string> phases)
    {
        var set = new HashSet<string>();
        foreach (var phase in phases)
        {
            if (!set.Add(phase))
            {
                throw new ArgumentException($"Duplicate phase: {phase}");
            }
        }
    }
}