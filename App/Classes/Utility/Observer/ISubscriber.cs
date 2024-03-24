namespace Bamboozlers.Classes.Utility.Observer;

public interface ISubscriber
{
    /// <summary>
    /// Asynchrously performs a given (implemented) action when called by the publisher this subscriber corresponds to.
    /// </summary>
    async Task OnUpdateAsync() { }

    /// <summary>
    /// Synchronously performs a given (implemented) action when called by the publisher this subscriber corresponds to.
    /// </summary>
    void OnUpdate() { }
}