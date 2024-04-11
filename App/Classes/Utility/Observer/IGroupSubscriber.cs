namespace Bamboozlers.Classes.Utility.Observer;

public enum GroupEvent
{
    General,
    CreateGroup,
    DeleteGroup,
    SentInvite,
    ReceivedInvite,
    ReceivedInviteAccepted,
    ReceivedInviteDeclined,
    ReceivedInviteRevoked,
    SentInviteRevoked,
    SentInvitePending,
    SelfLeftGroup,
    GrantedPermissions,
    RevokedPermissions,
    PermissionsLost,
    PermissionsGained,
    RemoveMember,
    GroupDisplayChange,
    OtherLeftGroup
    
    /* TODO: Implement methods in UserGroupService upon receiving network packet for this?
    SentInviteAccepted
    SentInviteDeclined
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
    Task OnUpdate(GroupEvent evt, int? specifiedGroup = null, int? specifiedUser = null);
}