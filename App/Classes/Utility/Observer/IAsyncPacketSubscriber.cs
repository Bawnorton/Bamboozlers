using Bamboozlers.Classes.Networking.Packets;

namespace Bamboozlers.Classes.Utility.Observer;

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