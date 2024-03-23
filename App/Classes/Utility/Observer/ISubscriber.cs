namespace Bamboozlers.Classes.Utility.Observer;

public interface ISubscriber
{
    /// <summary>
    /// Performs a given (implemented) action when called by the publisher this subscriber corresponds to.
    /// </summary>
    void OnUpdate();
}