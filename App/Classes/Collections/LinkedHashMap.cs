using Bamboozlers.Classes.AppDbContext;

namespace Bamboozlers.Classes.Collections;

public class LinkedHashMap<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, LinkedListNode<Tuple<TValue, TKey>>> _dictionary = new();
    private readonly LinkedList<Tuple<TValue, TKey>> _linkedList = [];

    public TValue? Get(TKey key)
    {
        return _dictionary.TryGetValue(key, out var node) ? node.Value.Item1 : default;
    }

    public void Add(TKey key, TValue value)
    {
        if (_dictionary.TryGetValue(key, out var node))
        {
            _linkedList.Remove(node);
        }
        
        _dictionary[key] = new LinkedListNode<Tuple<TValue, TKey>>(Tuple.Create(value, key));
        _linkedList.AddLast(_dictionary[key]);
    }
    
    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);
    
    public TValue PopFirst()
    {
        if (_linkedList.Count == 0)
        {
            throw new InvalidOperationException("The linked list is empty");
        }

        var first = _linkedList.First!;
        _linkedList.RemoveFirst();
        _dictionary.Remove(first.Value.Item2);
        return first.Value.Item1;
    }
    
    public int Count => _dictionary.Count;

    public TValue Remove(TKey key)
    {
        if (!_dictionary.TryGetValue(key, out var node))
        {
            throw new KeyNotFoundException("The key was not found in the dictionary");
        }

        _linkedList.Remove(node);
        _dictionary.Remove(key);
        return node.Value.Item1;
    }
}