using Bamboozlers.Classes.Collections;
using Bamboozlers.Classes.Collections.Toposort;
using Bamboozlers.Classes.Events.Api;

namespace Bamboozlers.Classes.Events.Impl;

public class ArrayBackedEvent<T> : Event<T>, IArrayBackedEvent
{
    private readonly Func<T[], T> _invokerFactory;
    private T[] _handlers;

    private readonly LinkedHashMap<string, EventPhaseData<T>> _phases = new();

    private readonly List<EventPhaseData<T>> _sortedPhases = [];
    
    public ArrayBackedEvent(Func<T[], T> invokerFactory)
    {
        _invokerFactory = invokerFactory;
        _handlers = Array.Empty<T>();
        Update();
    }

    public void Update()
    {
        invoker = _invokerFactory(_handlers);
    }
    
    public override void Register(T listener)
    {
        Register(DefaultPhase, listener);
    }

    private void Register(string phase, T listener)
    {
        ArgumentNullException.ThrowIfNull(phase, "Tried to register listener with null phase");
        ArgumentNullException.ThrowIfNull(listener, "Tried to register null listener");
        
        GetOrCreatePhase(phase, true).AddListener(listener);
        RebuildInvoker(_handlers.Length + 1);
    }
    
    private EventPhaseData<T> GetOrCreatePhase(string phase, bool sortIfCreate)
    {
        var phaseData = _phases.Get(phase);
        if (phaseData != null) return phaseData;
        
        phaseData = new EventPhaseData<T>(phase, typeof(T));
        _phases.Add(phase, phaseData);
        _sortedPhases.Add(phaseData);
            
        if (sortIfCreate)
        {
            NodeSorting.Sort(_sortedPhases, new EventPhaseComparer<T>());
        }

        return phaseData;
    }
    
    private void RebuildInvoker(int newLength)
    {
        if (_sortedPhases.Count == 1)
        {
            _handlers = _sortedPhases[0].Listeners;
        }
        else
        {
            var newHandlers = new T[newLength];
            var index = 0;
            
            foreach (var phase in _sortedPhases)
            {
                var length = phase.Listeners.Length;
                Array.Copy(phase.Listeners, 0, newHandlers, index, length);
                index += length;
            }
            
            _handlers = newHandlers;
        }
        
        Update();
    }
    
    public override void AddPhaseOrdering(string firstPhase, string secondPhase)
    {
        ArgumentNullException.ThrowIfNull(firstPhase, "Tried to add phase ordering with null first phase");
        ArgumentNullException.ThrowIfNull(secondPhase, "Tried to add phase ordering with null second phase");
        
        if (firstPhase == secondPhase)
        {
            throw new ArgumentException("Tried to add a phase that depends on itself");
        }
        
        var first = GetOrCreatePhase(firstPhase, false);
        var second = GetOrCreatePhase(secondPhase, false);
        EventPhaseData<T>.Link(first, second);
        NodeSorting.Sort(_sortedPhases, new EventPhaseComparer<T>());
        RebuildInvoker(_handlers.Length);
    }

    private class EventPhaseComparer<TE> : Comparer<EventPhaseData<TE>>
    {
        public override int Compare(EventPhaseData<TE>? x, EventPhaseData<TE>? y)
        {
            return x switch
            {
                null when y == null => 0,
                null => -1,
                _ => y == null ? 1 : string.Compare(x.ID, y.ID, StringComparison.Ordinal)
            };
        }
    }
}

public interface IArrayBackedEvent
{
    internal void Update();
}