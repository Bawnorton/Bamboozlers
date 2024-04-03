namespace Bamboozlers.Classes.Collections.Toposort;

public abstract class SortableNode<T> where T : SortableNode<T>
{
    internal readonly List<T> PreviousNodes = [];
    internal readonly List<T> SubsequntNodes = [];
    internal bool Visited = false;

    protected abstract string GetDescription();

    public static void Link<TN>(TN first, TN second) where TN : SortableNode<TN>
    {
        if (first == second) throw new ArgumentException("Cannot link a node to itself");

        first.SubsequntNodes.Add(second);
        second.PreviousNodes.Add(first);
    }
}