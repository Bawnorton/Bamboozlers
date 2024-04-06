using Bamboozlers.Classes.Networking.Packets;
using Bamboozlers.Classes.Services;

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

public interface IAsyncUserSubscriber : IAsyncSubscriber
{
    /// <summary>
    ///     Asynchronously performs a given (implemented) action when called by the publisher this subscriber corresponds to.
    /// </summary>
    Task OnUserUpdate();
}

public interface IAsyncInteractionSubscriber : IAsyncSubscriber
{
    /// <summary>
    ///     Asynchronously performs a given (implemented) action when called by the publisher this subscriber corresponds to.
    /// </summary>
    Task OnInteractionUpdate();
}

public interface IAsyncGroupSubscriber : IAsyncSubscriber
{
    List<int?> WatchedIDs { get; }

    /// <summary>
    ///     Asynchronously performs a given (implemented) action when called by the publisher this subscriber corresponds to.
    /// </summary>
    Task OnGroupUpdate();
}

public interface IAsyncPacketSubscriber : IAsyncSubscriber
{
    /// <summary>
    ///     Asynchronously performs a given (implemented) action when called by the publisher this subscriber corresponds to.
    /// </summary>
    Task OnPacket(IPacket packet);

    Task IAsyncSubscriber.OnUpdate()
    {
        throw new NotImplementedException();
    }
}

public interface IAsyncKeySubscriber : IAsyncSubscriber
{
    Task OnKeyPressed(KeyEventArgs keyEventArgs);
    Task OnKeyReleased(KeyEventArgs keyEventArgs);
}