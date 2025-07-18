using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Ql_NhaTro_jun.Hubs
{
    public class ChatHub : Hub
    {
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        public async Task SendMessage(string senderId, string receiverId, string message, string senderName)
        {
            // Gửi tin nhắn đến người nhận
            await Clients.Group($"user_{receiverId}").SendAsync("ReceiveMessage", senderId, message, senderName);
            
            // Gửi tin nhắn về cho người gửi để confirm
            await Clients.Group($"user_{senderId}").SendAsync("MessageSent", receiverId, message);
        }

        public async Task MarkAsRead(string currentUserId, string senderId)
        {
            // Thông báo cho người gửi biết tin nhắn đã được đọc
            await Clients.Group($"user_{senderId}").SendAsync("MessageRead", currentUserId);
        }

        public async Task Typing(string senderId, string receiverId, bool isTyping)
        {
            await Clients.Group($"user_{receiverId}").SendAsync("UserTyping", senderId, isTyping);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
} 