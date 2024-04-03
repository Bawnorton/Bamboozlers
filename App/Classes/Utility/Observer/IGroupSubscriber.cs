namespace Bamboozlers.Classes.Utility.Observer;

public enum GroupEvent
{
    General,
    CreateGroup,
    SentInvite,
    ReceivedInviteAccepted,
    ReceivedInviteDeclined,
    SentInviteRevoked,
    SentInvitePending,
    SelfLeftGroup,
    GrantedPermissions,
    RevokedPermissions,
    RemoveMember,
    GroupDisplayChange,
    
    /* TODO: Implement methods in UserGroupService upon receiving network packet for this?
    ReceivedInviteRevoked
    SentInviteAccepted
    SentInviteDeclined
    OtherLeftGroup
    SelfPermissionsRevoked
    SelfPermissionsGranted 
    */
}

public interface IGroupSubscriber : IAsyncSubscriber
{
    List<int?> WatchedIDs { get; }
    List<GroupEvent> WatchedGroupEvents { get; }

    /// <summary>
    /// Asynchronously performs a given (implemented) action when called by the publisher this subscriber corresponds to.
    /// </summary>
    Task OnUpdate(GroupEvent evt, int? specifiedGroup = null);
}