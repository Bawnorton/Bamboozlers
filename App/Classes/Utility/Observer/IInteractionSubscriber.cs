namespace Bamboozlers.Classes.Utility.Observer;

public enum InteractionEvent
{
    General,
    CreateDm,
    RequestAccepted,
    RequestDeclined,
    RequestSent,
    RequestRevoked,
    RequestPending,
    RequestReceived,
    Unfriend,
    Block,
    Unblock
    
    /* TODO: Implement methods in UserInteractionService upon receiving network packet for this?
     ReceivedRequest
     RequestCancelled
     SentRequestAccepted
     SentRequestDeclined
     Unfriended
     Blocked
     Unblocked
     */
}

public interface IInteractionSubscriber : IAsyncSubscriber
{
    List<InteractionEvent> WatchedInteractionEvents { get; }
    /// <summary>
    /// Asynchronously performs a given (implemented) action when called by the publisher this subscriber corresponds to.
    /// </summary>
    Task OnUpdate(InteractionEvent evt);
}