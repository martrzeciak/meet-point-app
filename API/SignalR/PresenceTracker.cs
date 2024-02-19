namespace API.SignalR
{
    public class PresenceTracker
    {
        private static readonly Dictionary<string, List<string>> OnlineUsers =
            new Dictionary<string, List<string>>();

        public bool UserConnected(string username, string connectionId)
        {
            bool isUserOnline = false;

            if (OnlineUsers.ContainsKey(username))
            {
                OnlineUsers[username].Add(connectionId);
            }
            else
            {
                OnlineUsers.Add(username, new List<string> { connectionId });
                isUserOnline = true;
            }

            return isUserOnline;
        }

        public bool UserDisconnected(string username, string connectionId)
        {
            bool isUserOffline = false;

            if (!OnlineUsers.ContainsKey(username)) return isUserOffline;

            OnlineUsers[username].Remove(connectionId);

            if (OnlineUsers[username].Count == 0)
            {
                OnlineUsers.Remove(username);
                isUserOffline = true;
            }

            return isUserOffline;
        }

        public string[] GetConnectedUsers()
        {
            string[] onlineUsers;

            onlineUsers = OnlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();

            return onlineUsers;
        }

        public static List<string> GetConnectionsForUserc(string username)
        {
            List<string> connectionIds;

            connectionIds = OnlineUsers.GetValueOrDefault(username);

            return connectionIds;
        }
    }
}
