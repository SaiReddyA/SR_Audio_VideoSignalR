using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SReddy_Audio_Video.SaiChatHub
{
    public class SaiReddyChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> ActiveUsers = new();

        // When a user connects
        public override async Task OnConnectedAsync()
        {
            string userName = Context.User?.Identity?.Name ?? Context.ConnectionId;
            ActiveUsers.TryAdd(Context.ConnectionId, userName);

            await Clients.All.SendAsync("UserJoined", userName);
            await Clients.All.SendAsync("UpdateActiveUsers", ActiveUsers.Values);
            await base.OnConnectedAsync();
        }

        // When a user disconnects
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (ActiveUsers.TryRemove(Context.ConnectionId, out string userName))
            {
                await Clients.All.SendAsync("UserLeft", userName);
                await Clients.All.SendAsync("UpdateActiveUsers", ActiveUsers.Values);
            }
            await base.OnDisconnectedAsync(exception);
        }

        // Send a message to a specific user (1-to-1 chat)
        public async Task SendMessageToUser(string toUser, string message)
        {
            var fromUser = ActiveUsers.GetValueOrDefault(Context.ConnectionId);
            var recipient = ActiveUsers.FirstOrDefault(x => x.Value == toUser).Key;

            if (!string.IsNullOrEmpty(recipient))
            {
                await Clients.Client(recipient).SendAsync("ReceiveMessage", fromUser, message);
                await Clients.Caller.SendAsync("ReceiveMessage", fromUser, message); // Show message to sender
            }
        }

        // Send a message to a group (Group Chat)
        public async Task SendMessageToGroup(string groupName, string message)
        {
            string user = ActiveUsers.GetValueOrDefault(Context.ConnectionId);
            await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", user, message);
        }

        // Join a group
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            string user = ActiveUsers.GetValueOrDefault(Context.ConnectionId);
            await Clients.Group(groupName).SendAsync("GroupUserJoined", user);
        }

        // Leave a group
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            string user = ActiveUsers.GetValueOrDefault(Context.ConnectionId);
            await Clients.Group(groupName).SendAsync("GroupUserLeft", user);
        }

        // Typing Indicator
        public async Task Typing(string toUser)
        {
            var fromUser = ActiveUsers.GetValueOrDefault(Context.ConnectionId);
            var recipient = ActiveUsers.FirstOrDefault(x => x.Value == toUser).Key;

            if (!string.IsNullOrEmpty(recipient))
            {
                await Clients.Client(recipient).SendAsync("UserTyping", fromUser);
            }
        }

        // Stop Typing Indicator
        public async Task StopTyping(string toUser)
        {
            var fromUser = ActiveUsers.GetValueOrDefault(Context.ConnectionId);
            var recipient = ActiveUsers.FirstOrDefault(x => x.Value == toUser).Key;

            if (!string.IsNullOrEmpty(recipient))
            {
                await Clients.Client(recipient).SendAsync("UserStoppedTyping", fromUser);
            }
        }
    }
}
