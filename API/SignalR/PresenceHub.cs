using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private  readonly PresenceTracker _presenceTracker;
        public PresenceHub(PresenceTracker presenceTracker)
        {
            _presenceTracker = presenceTracker;
        }

        public override async Task OnConnectedAsync()
        {
            bool isUserOnline = _presenceTracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);

            if (isUserOnline)
            {
                await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUsername());
            }
            
            var currentUsers = _presenceTracker.GetConnectedUsers();
            await Clients.Caller.SendAsync("GetConnectedUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            bool isUserOffline = _presenceTracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);

            if (isUserOffline)
            {
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUsername());
            }

            //var currentUsers = _presenceTracker.GetConnectedUsers();
            //await Clients.All.SendAsync("GetConnectedUsers", currentUsers);

            await base.OnDisconnectedAsync(exception);
        }
    }
}
