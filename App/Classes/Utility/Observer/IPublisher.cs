namespace Bamboozlers.Classes.Utility.Observer;

public interface IPublisher<T>
{
    List<T> Subscribers { get; }

    /// <summary>
    /// Adds the given subscriber to this publisher's list of subscribers.
    /// </summary>
    /// <param name="subscriber">The subscriber to be added.</param>
    /// <returns>A boolean indicating whether the subscriber was added to the list.</returns>
    bool AddSubscriber(T subscriber);
    
    /// <summary>
    /// Removes, if present, the given subscriber from this publisher's list of subscribers.
    /// </summary>
    /// <param name="subscriber">The subscriber to be removed.</param>
    /// <returns>A boolean indicating whether the subscriber was present in the list and removed.</returns>
    bool RemoveSubscriber(T subscriber)
    {
        return Subscribers.Remove(subscriber);
    }

    /// <summary>
    /// Goes through each subscriber to this publisher and calls their update method.
    /// </summary>
    void NotifyAll();
}

public interface IAsyncPublisher<T>
{
    List<T> Subscribers { get; }

    /// <summary>
    /// Adds the given subscriber to this publisher's list of subscribers.
    /// </summary>
    /// <param name="subscriber">The subscriber to be added.</param>
    /// <returns>A boolean indicating whether the subscriber was added to the list.</returns>
    bool AddSubscriber(T subscriber);
    
    /// <summary>
    /// Removes, if present, the given subscriber from this publisher's list of subscribers.
    /// </summary>
    /// <param name="subscriber">The subscriber to be removed.</param>
    /// <returns>A boolean indicating whether the subscriber was present in the list and removed.</returns>
    bool RemoveSubscriber(T subscriber)
    {
        return Subscribers.Remove(subscriber);
    }

    /// <summary>
    /// Goes through each subscriber to this publisher and calls their update method.
    /// </summary>
    Task NotifyAllAsync();
}