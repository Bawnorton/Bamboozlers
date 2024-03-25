using Bamboozlers.Classes.Collections;
using Bamboozlers.Classes.Collections.Toposort;
using Bamboozlers.Classes.Events.Api;

namespace Bamboozlers.Classes.Events.Impl;

public class ArrayBackedEvent<T> : Event<T>, IArrayBackedEvent
{
    private readonly Func<T[], T> _invokerFactory;
    private Dictionary<string, T> _handlers;

    private readonly LinkedHashMap<string, EventPhaseData<T>> _phases = new();

    private readonly List<EventPhaseData<T>> _sortedPhases = [];
    
    public ArrayBackedEvent(Func<T[], T> invokerFactory)
    {
        _invokerFactory = invokerFactory;
        _handlers = new Dictionary<string, T>();
        Update();
    }

    public void Update()
    {
        invoker = _invokerFactory(_handlers.Values.ToArray());
    }
    
    public override void Register(string id, T listener)
    {
        Register(id, DefaultPhase, listener);
    }

    private void Register(string id, string phase, T listener)
    {
        ArgumentNullException.ThrowIfNull(phase, "Tried to register listener with null phase");
        ArgumentNullException.ThrowIfNull(listener, "Tried to register null listener");
        
        GetOrCreatePhase(phase, true).AddListener(id, listener);
        RebuildInvoker();
    }
    
    private EventPhaseData<T> GetOrCreatePhase(string phase, bool sortIfCreate)
    {
        var phaseData = _phases.Get(phase);
        if (phaseData != null) return phaseData;
        
        phaseData = new EventPhaseData<T>(phase);
        _phases.Add(phase, phaseData);
        _sortedPhases.Add(phaseData);
            
        if (sortIfCreate)
        {
            NodeSorting.Sort(_sortedPhases, new EventPhaseComparer<T>());
        }

        return phaseData;
    }
    
    private void RebuildInvoker()
    {
        if (_sortedPhases.Count == 1)
        {
            _handlers = _sortedPhases[0].Listeners;
        }
        else
        {
            var newHandlers = new Dictionary<string, T>();
            foreach (var phase in _sortedPhases)
            {
                foreach (var (key, value) in phase.Listeners)
                {
                    newHandlers[key] = value;
                }
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
        RebuildInvoker();
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