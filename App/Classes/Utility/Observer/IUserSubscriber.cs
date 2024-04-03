using Bamboozlers.Classes.Data;

namespace Bamboozlers.Classes.Utility.Observer;

public interface IUserSubscriber : ISubscriber
{
    /// <summary>
    /// Asynchronously performs a given (implemented) action when called by the publisher this subscriber corresponds to.
    /// </summary>
    void OnUpdate(UserRecord? data);
}