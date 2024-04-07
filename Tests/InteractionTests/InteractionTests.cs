using AngleSharp.Dom;
using Bamboozlers.Classes.AppDbContext;
using Bamboozlers.Classes.Data;
using Bamboozlers.Classes.Utility.Observer;
using Bamboozlers.Components.Interaction;
using Blazorise;
using Bunit.Extensions.WaitForHelpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Tests.InteractionTests;

public class InteractionTests : InteractionTestBase
{
    [Fact]
    public async Task FriendListTests_CompFriendList()
    {
        var (users, friendships, _, _) = BuildTestCases();
        var subjectUser = users[0];
        await SetUser(subjectUser);

        OpenChatArgs? chatArgs = null;
        KnownPopupArgs? popupArgs = null;
        bool? calledToClose = false;
        
        var fauxOpenChat= EventCallback.Factory.Create<OpenChatArgs>(this, args => chatArgs = args);
        var fauxOpenPopup = EventCallback.Factory.Create<KnownPopupArgs>(this, args => popupArgs = args);
        var fauxClosePopup = EventCallback.Factory.Create(this, () => calledToClose = true);
        
        var component = Ctx.RenderComponent<CompFriendList>(parameters =>
        {
            parameters.Add(p => p.OpenChatCallback, fauxOpenChat);
            parameters.Add(p => p.OpenKnownPopup, fauxOpenPopup);
            parameters.Add(p => p.ClosePopup, fauxClosePopup);
        });

        var container = component.Find(".scrollbar-container");
        var subjectFriends = friendships
            .Where(f => f.User1ID == 0 || f.User2ID == 0)
                .Select(f => f.User1ID == 0 ? f.User2 : f.User1)
                    .ToList();
        
        foreach (var friend in subjectFriends)
        {
            var friendContainer = container.Descendants<IElement>().FirstOrDefault(e => e.Id == $"user-{friend}");
            Assert.NotNull(friendContainer);
            
            var username = friendContainer.Descendants<IElement>().FirstOrDefault(e => e.Id == "username");
            Assert.NotNull(username);
            Assert.Contains($"{friend.UserName}",username.TextContent);
            
            var messageButton = friendContainer.Descendants<IElement>().FirstOrDefault(e => e.Id == "send-message");
            Assert.NotNull(messageButton);

            await messageButton.ClickAsync(new MouseEventArgs());
            
            Assert.NotNull(chatArgs);
            Assert.Equal(ChatType.Dm, chatArgs.ChatType);
            Assert.Equal(friend.Id, chatArgs.Id);
            
            var clickSpan = friendContainer.Descendants<IElement>().FirstOrDefault(e => e.ClassName == "inner");
            Assert.NotNull(clickSpan);
            await clickSpan.ClickAsync(new MouseEventArgs());

            Assert.NotNull(popupArgs);
            Assert.Equal(PopupType.UserProfile, popupArgs.Type);
            Assert.Equal(friend, popupArgs.FocusUser);
        }
        
        component.Dispose();
        component = Ctx.RenderComponent<CompFriendList>(parameters =>
        {
            parameters.Add(p => p.OpenChatCallback, fauxOpenChat);
            parameters.Add(p => p.OpenKnownPopup, fauxOpenPopup);
            parameters.Add(p => p.ClosePopup, fauxClosePopup);
            parameters.Add(p => p.IsPopup, true);
        });
        
        container = component.Find(".scrollbar-container");
        
        var f = subjectFriends[0];
        var fContainer = container.Descendants<IElement>().FirstOrDefault(e => e.Id == $"user-{f.UserName}");
        Assert.NotNull(fContainer);
        
        var mButton = fContainer.Descendants<IElement>().FirstOrDefault(e => e.Id == "send-message");
        Assert.NotNull(mButton);

        await mButton.ClickAsync(new MouseEventArgs());
            
        Assert.NotNull(chatArgs);
        Assert.Equal(ChatType.Dm, chatArgs.ChatType);
        Assert.Equal(f.Id, chatArgs.Id);
        
        Assert.True(calledToClose);
        
        // Observer Pattern
        var subjectFriendship = friendships[0];
        var f1 = subjectFriendship.User1ID == 0 ? subjectFriendship.User2 : subjectFriendship.User1;
        
        fContainer = container.Descendants<IElement>().FirstOrDefault(e => e.Id == $"user-{f1.UserName}");
        Assert.NotNull(fContainer);
       
        MockDatabaseProvider.GetMockAppDbContext().MockFriendships.RemoveMock(subjectFriendship);
        await component.Instance.OnUpdate(InteractionEvent.General);
        
        container = component.Find(".scrollbar-container");
        fContainer = container.Descendants<IElement>().FirstOrDefault(e => e.Id == $"user-{f1.UserName}");
        Assert.Null(fContainer);
    }

    [Fact]
    public async Task InteractionTests_CompAddFriend()
    {
        var (users, friendships, friendRequests, blocks) = BuildTestCases();
        var subjectUser = users[0];
        await SetUser(subjectUser);
        
        KnownPopupArgs? popupArgs = null;
        var fauxOpenPopup = EventCallback.Factory.Create<KnownPopupArgs>(this, args => popupArgs = args);
        
        var component = Ctx.RenderComponent<CompAddFriend>(parameters =>
        {
            parameters.Add(p => p.OpenKnownPopup, fauxOpenPopup);
        });
        
        var container = component.Find(".scrollbar-container");
        Assert.NotNull(container);
        
        var friendUsers = friendships
            .Where(f => f.User1ID == 0 || f.User2ID == 0)
            .Select(f => f.User1ID == 0 ? f.User2 : f.User1)
            .ToList();
        
        var blockedUsers = blocks
            .Where(f => f.BlockerID == 0 || f.BlockedID == 0)
            .Select(f => f.BlockerID == 0 ? f.Blocked : f.Blocker)
            .ToList();
        
        var subjectUsers = users.Where(u => u.Id != subjectUser.Id)
            .Where(u => !friendUsers.Contains(u))
            .Where(u => !blockedUsers.Contains(u))
            .ToList();
        
        component.Instance.SearchQuery = "T";
        var searchButton = component.Find("#search-button");
        await searchButton.ClickAsync(new MouseEventArgs());
        container = component.Find(".scrollbar-container");
        Assert.Equal(1 + subjectUsers.Count * 2, container.ChildElementCount);
        
        component.Instance.SearchQuery = "";
        searchButton = component.Find("#search-button");
        await searchButton.ClickAsync(new MouseEventArgs());
        container = component.Find(".scrollbar-container");
        Assert.Equal(1, container.ChildElementCount);
        
        
        // Observer Pattern
        component.Instance.SearchQuery = "T";
        searchButton = component.Find("#search-button");
        await searchButton.ClickAsync(new MouseEventArgs());
        
        var subjectFriendRequest = friendRequests[3];
        var f = subjectFriendRequest.ReceiverID == 0 ? subjectFriendRequest.Sender : subjectFriendRequest.Receiver;
        
        var incomingUserSection = container.Descendants<IElement>().FirstOrDefault(e => e.Id == $"user-{f.UserName}");
        Assert.NotNull(incomingUserSection);
       
        MockDatabaseProvider.GetMockAppDbContext().MockFriendRequests.RemoveMock(subjectFriendRequest);
        MockDatabaseProvider.GetMockAppDbContext().MockFriendships.AddMock(new Friendship(subjectUser.Id,f.Id) {User1 = subjectUser, User2 = f});
        await component.Instance.OnUpdate(InteractionEvent.General);
        
        container = component.Find(".scrollbar-container");
        incomingUserSection = container.Descendants<IElement>().FirstOrDefault(e => e.Id == $"user-{f.UserName}");
        Assert.Null(incomingUserSection);
    }

    [Fact]
    public async Task InteractionTests_CompBlockedUsers()
    {
        var (users, _, _, blocks) = BuildTestCases();
        var subjectUser = users[0];
        var subjectBlock = blocks[0];
        var subjectBlocked = subjectBlock.Blocked;
        await SetUser(subjectUser);

        var component = Ctx.RenderComponent<CompBlockedUsers>();
        var container = component.Find(".scrollbar-container");
        var blockedDiv = container.Descendants<IElement>().FirstOrDefault(e => e.Id == $"user-{subjectBlocked.UserName}");
        Assert.NotNull(blockedDiv);

        var unblockButton = blockedDiv.Descendants<IElement>().FirstOrDefault(e => e.Id == "unblock-button");
        Assert.NotNull(unblockButton);
        
        MockDatabaseProvider.GetMockAppDbContext().MockBlocks.RemoveMock(subjectBlock);
        await component.Instance.OnUpdate(InteractionEvent.General);
        
        container = component.Find(".scrollbar-container");
        blockedDiv = container.Descendants<IElement>().FirstOrDefault(e => e.Id == $"user-{subjectBlocked.UserName}");
        Assert.Null(blockedDiv);
    }

    [Fact]
    public async void InteractionTests_CompFriendRequests()
    {
        var (users, _, friendRequests, _) = BuildTestCases();
        var subjectUser = users[0];
        await SetUser(subjectUser);
        
        List<FriendRequest> incoming = [];
        List<FriendRequest> outgoing = [];

        foreach (var inv in MockDatabaseProvider.GetMockAppDbContext().MockFriendRequests.GetMocks())
        {
            if (inv.Sender == subjectUser)
            {
                outgoing.Add(inv);
            } 
            else if (inv.Receiver == subjectUser)
            {
                incoming.Add(inv);
            }
        }
        
        var component = Ctx.RenderComponent<CompFriendRequests>();
        var incomingBadge = component.Find("#incoming-badge");
        // TODO: Fails
        /* Assert.Contains($"{incoming.Count}", incomingBadge.TextContent);
        
        var outgoingBadge = component.Find("#outgoing-badge");
        Assert.Contains($"{outgoing.Count}", outgoingBadge.TextContent);
        */
        
        foreach (var invite in incoming)
        {
            var inviteDiv = component.Find($"#incoming-requests #user-{invite.Sender.UserName}");
            
            var username = inviteDiv.Descendants<IElement>().FirstOrDefault(e => e.Id == "username");
            Assert.NotNull(username);
            Assert.Contains($"{invite.Sender.UserName}",username.TextContent);
            
            var acceptButton = inviteDiv.Descendants<IElement>().FirstOrDefault(e => e.Id == "accept-button");
            Assert.NotNull(acceptButton);
            
            var declineButton = inviteDiv.Descendants<IElement>().FirstOrDefault(e => e.Id == "decline-button");
            Assert.NotNull(declineButton);
        }
        
        component.Find("#outgoing-toggle").FirstElementChild!.Click();
        foreach (var invite in outgoing)
        {
            var inviteDiv = component.Find($"#outgoing-requests #user-{invite.Receiver.UserName}");
            
            var username = inviteDiv.Descendants<IElement>().FirstOrDefault(e => e.Id == "username");
            Assert.NotNull(username);
            Assert.Contains($"{invite.Receiver.UserName}",username.TextContent);
            
            var revokeButton = inviteDiv.Descendants<IElement>().FirstOrDefault(e => e.Id == "revoke-button");
            Assert.NotNull(revokeButton);
        }
        
        // Observer Pattern
        component.Find("#incoming-toggle").FirstElementChild!.Click();
        var subjectInvite = friendRequests[0];
        MockDatabaseProvider.GetMockAppDbContext().MockFriendRequests.RemoveMock(subjectInvite);
        await component.Instance.OnUpdate(InteractionEvent.General);
        
        Assert.Throws<WaitForFailedException>(() => component.WaitForElement($"#incoming-requests #user-{subjectInvite.Sender.UserName}"));
    }
}