using Bamboozlers.Classes.Collections.Toposort;

namespace Bamboozlers.Classes.Events.Impl;

public class EventPhaseData<T>(string id, Type listenerType) : SortableNode<EventPhaseData<T>>
{
    internal T[] Listeners = (T[]) Array.CreateInstance(listenerType, 0);

    internal readonly string ID = id;

    public void AddListener(T listener)
    {
        var length = Listeners.Length;
        Array.Resize(ref Listeners, length + 1);
        Listeners[length] = listener;
    }

    protected override string GetDescription()
    {
        return ID;
    }
}