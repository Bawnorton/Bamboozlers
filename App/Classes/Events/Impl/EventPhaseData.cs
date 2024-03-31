using Bamboozlers.Classes.Collections.Toposort;

namespace Bamboozlers.Classes.Events.Impl;

public class EventPhaseData<T>(string id) : SortableNode<EventPhaseData<T>>
{
    internal readonly Dictionary<string, T> Listeners = new();

    internal readonly string ID = id;

    public void AddListener(string id, T listener)
    {
        Listeners[id] = listener;
    }

    protected override string GetDescription()
    {
        return ID;
    }
}