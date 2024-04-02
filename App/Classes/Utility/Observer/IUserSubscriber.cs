namespace Bamboozlers.Classes.Utility.Observer;

public interface IUserSubscriber : IAsyncSubscriber
{
    /// <summary>
    /// Asynchronously performs a given (implemented) action when called by the publisher this subscriber corresponds to.
    /// </summary>
    Task OnUserUpdate();
}