using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Func;
using Microsoft.AspNetCore.Components;

namespace Bamboozlers.Classes.Data;
public enum PopupType
{
    Settings,
    UserProfile,
    FriendList,
    CreateGroup,
    GroupChatSettings,
    InviteGroupMembers,
    FindFriends,
    Pins
}

public record KnownPopupArgs(PopupType Type, User? FocusUser = null, int? ChatId = null, AsyncConsumer<object?>? OnClose = null);
public record NewPopupArgs(RenderFragment Body, RenderFragment? Header = null, RenderFragment? Footer = null);
public record AlertPopupArgs(RenderFragment AlertBody, EventCallback OnConfirmCallback);

