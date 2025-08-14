using MessagesModels.Models;
using SFU_MainCluster.SFU.Tools;

namespace SFU_MainCluster.SFU.Main.Server.Coordinator
{
    public partial class Coordinator
    {
        internal void SendMessageToUser(string userId, BaseMessage message)
        {
            if (AssociatedSockets[userId].IsAvailable)
            {
                AssociatedSockets[userId].Send(ServerTools.SerializeMessage(message));
            }
        }
        
        internal void BroadCastMessageToUsers(BaseMessage message)
        {
            foreach (var user in ConnectedUsers)
            {
                if (user.Value != null && AssociatedSockets[user.Value.Id].IsAvailable)
                {
                    AssociatedSockets[user.Value.Id].Send(ServerTools.SerializeMessage(message));
                }
            }
        } // Не реализованно!
    }
}
