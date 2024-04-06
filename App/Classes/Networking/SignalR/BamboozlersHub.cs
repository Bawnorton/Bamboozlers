using Bamboozlers.Classes.Networking.Packets;
using Bamboozlers.Classes.Networking.Packets.Clientbound;
using Bamboozlers.Classes.Networking.Packets.Serverbound;
using Microsoft.AspNetCore.SignalR;

namespace Bamboozlers.Classes.Networking.SignalR;

/*
 * Normally you'd auth something like this, which would also give the convenience of using the User property in the Hub
 * Alas, 4 year old issue that is still open
 * https://github.com/dotnet/aspnetcore/issues/25000
 * The workarounds mentioned in the issue do not work with our current setup (Because of course not), I tried all of them
 */

// ReSharper disable UnusedMember.Global - This class is used by the SignalR framework
public class BamboozlersHub : Hub
{
    public const string HubUrl = "/bamboozlers_chat";

    public async Task ReceivePacketOnServer(string packetJson)
    {
        await ServerNetworkHandler.Instance.Handle(packetJson, async packet =>
        {
            Console.WriteLine($"Recieved packet from client: {packet.PacketType().GetId()}");
            switch (packet)
            {
                case JoinChatC2SPacket joinChat:
                    await Groups.AddToGroupAsync(Context.ConnectionId, joinChat.ChatId.ToString());
                    break;
                case MessageSentC2SPacket messageSent:
                    var messageSentResponse = new MessageSentS2CPacket
                    {
                        MessageId = messageSent.MessageId
                    };
                    await SendToChat(messageSent.ChatId, messageSentResponse);
                    break;
                case MessageEditedC2SPacket messageEdited:
                    var messageEditedResponse = new MessageEditedS2CPacket
                    {
                        MessageId = messageEdited.MessageId,
                        NewContent = messageEdited.NewContent
                    };
                    await SendToChat(messageEdited.ChatId, messageEditedResponse);
                    break;
                case MessageDeletedC2SPacket messageDeleted:
                    var messageDeletedRespone = new MessageDeletedS2CPacket
                    {
                        MessageId = messageDeleted.MessageId
                    };
                    await SendToChat(messageDeleted.ChatId, messageDeletedRespone);
                    break;
                case MessagePinStatusC2SPacket messagePinStatus:
                    var messagePinStatusResponse = new MessagePinStatusS2CPacket
                    {
                        MessageId = messagePinStatus.MessageId,
                        Pinned = messagePinStatus.Pinned
                    };
                    await SendToChat(messagePinStatus.ChatId, messagePinStatusResponse);
                    break;
            }
        });
    }
    
    private async Task SendToChat(int chatId, IPacket packet)
    {
        Console.WriteLine($"Sending packet to chat {chatId}: {packet.PacketType().GetId()}");
        await Clients.Groups(chatId.ToString()).SendAsync("RecievePacketOnClient", packet.Serialize());
    }

    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        return base.OnDisconnectedAsync(exception);
    }
}