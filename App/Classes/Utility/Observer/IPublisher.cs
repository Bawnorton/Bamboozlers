namespace Bamboozlers.Classes.Utility.Observer;

public interface IPublisher
{
    List<ISubscriber> Subscribers { get; }

    /// <summary>
    /// Adds the given subscriber to this publisher's list of subscribers.
    /// </summary>
    /// <param name="subscriber">The subscriber to be added.</param>
    /// <returns>A boolean indicating whether the subscriber was added to the list.</returns>
    bool AddSubscriber(ISubscriber subscriber);
    
    /// <summary>
    /// Removes, if present, the given subscriber from this publisher's list of subscribers.
    /// </summary>
    /// <param name="subscriber">The subscriber to be removed.</param>
    /// <returns>A boolean indicating whether the subscriber was present in the list and removed.</returns>
    bool RemoveSubscriber(ISubscriber subscriber)
    {
        return Subscribers.Remove(subscriber);
    }

    /// <summary>
    /// Goes through each subscriber to this publisher and calls their update method.
    /// </summary>
    void NotifyAll();
}

public interface IAsyncPublisher
{
    List<IAsyncSubscriber> Subscribers { get; }

    /// <summary>
    /// Adds the given subscriber to this publisher's list of subscribers.
    /// </summary>
    /// <param name="subscriber">The subscriber to be added.</param>
    /// <returns>A boolean indicating whether the subscriber was added to the list.</returns>
    bool AddSubscriber(IAsyncSubscriber subscriber);
    
    /// <summary>
    /// Removes, if present, the given subscriber from this publisher's list of subscribers.
    /// </summary>
    /// <param name="subscriber">The subscriber to be removed.</param>
    /// <returns>A boolean indicating whether the subscriber was present in the list and removed.</returns>
    bool RemoveSubscriber(IAsyncSubscriber subscriber)
    {
        return Subscribers.Remove(subscriber);
    }

    /// <summary>
    /// Goes through each subscriber to this publisher and calls their update method.
    /// </summary>
    Task NotifyAllAsync();
}