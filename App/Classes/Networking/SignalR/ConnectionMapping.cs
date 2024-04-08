namespace Bamboozlers.Classes.Networking.SignalR;

public class ConnectionMapping<T> where T : notnull
{
    private readonly Dictionary<T, HashSet<string>> _connections = new();

    public int Count
    {
        get
        {
            lock (_connections)
            {
                return _connections.Count;
            }
        }
    }

    public void Add(T key, string connectionId)
    {
        lock (_connections)
        {
            if (!_connections.TryGetValue(key, out var connections))
            {
                connections = new HashSet<string>();
                _connections.Add(key, connections);
            }

            lock (connections)
            {
                connections.Add(connectionId);
            }
        }
    }

    public IEnumerable<string> GetConnections(T key)
    {
        lock (_connections)
        {
            return _connections.TryGetValue(key, out var connections) ? connections : Enumerable.Empty<string>();
        }
    }

    public void Remove(T key, string connectionId)
    {
        lock (_connections)
        {
            if (!_connections.TryGetValue(key, out var connections)) return;
            lock (connections)
            {
                connections.Remove(connectionId);
                if (connections.Count == 0) _connections.Remove(key);
            }
        }
    }

    public void RemoveConnection(string contextConnectionId)
    {
        lock (_connections)
        {
            var key = _connections.FirstOrDefault(x => x.Value.Contains(contextConnectionId)).Key;
            if (key == null) return;
            
            lock (_connections[key])
            {
                _connections.Remove(key);
            }
        }
    }
}