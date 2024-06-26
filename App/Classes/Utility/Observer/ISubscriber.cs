namespace Bamboozlers.Classes.Utility.Observer;

public interface ISubscriber
{
    /// <summary>
    ///     Synchronously performs a given (implemented) action when called by the publisher this subscriber corresponds to.
    /// </summary>
    void OnUpdate();
}

public interface IAsyncSubscriber
{
    /// <summary>
    ///     Asynchronously performs a given (implemented) action when called by the publisher this subscriber corresponds to.
    /// </summary>
    Task OnUpdate();
}