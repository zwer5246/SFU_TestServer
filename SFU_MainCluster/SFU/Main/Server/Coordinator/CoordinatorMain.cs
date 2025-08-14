using Fleck;
using MessagesModels.Enums;
using MessagesModels.Models;
using MessagesModels.Models.Requests;
using SFU_MainCluster.SFU.Main.Server.Coordinator.MessagesProcessing.Interfaces;
using SFU_MainCluster.SFU.Main.Server.Coordinator.MessagesProcessing.MessagesValidator;
using SFU_MainCluster.SFU.Main.Server.Video;
using SFU_MainCluster.SFU.Tools;

namespace SFU_MainCluster.SFU.Main.Server.Coordinator
{
    public partial class Coordinator
    {
        public Coordinator() 
        {
            Sessions = new Dictionary<string, VideoSession>();
            ConnectedUsers = new Dictionary<string, UserModel?>();
            AssociatedSockets = new Dictionary<string, IWebSocketConnection>();
            ServerTools.Logger.LogInformation("Server coordinator ready for connections.");
        }
        
        public void RegisterHandler(MessageType type, IMessageHandler handler)
        {
            _handlers[type] = handler;
        }
        
        public bool AddUserToInstance(UserModel userModel, IWebSocketConnection userSocket)
        {
            if (!ConnectedUsers.TryAdd(userModel.Id, userModel))
            {
                ServerTools.Logger.LogWarning($"User with id {userModel.Id} already connected. Replacing...");
                ConnectedUsers[userModel.Id] = userModel;
                AssociatedSockets[userModel.Id] = userSocket;

                UserAuthRequestModel userAddedToSessionMessage = new UserAuthRequestModel(UserAuthRequestModel.FlowRateMessageAuthStatus.Completed);

                SendMessageToUser(userModel.Id, new BaseMessage(userModel.Id, MessageType.UserAuth, userAddedToSessionMessage));
            }
            else
            {
                AssociatedSockets.Add(userModel.Id, userSocket);

                UserAuthRequestModel userAddedToSessionMessage = new UserAuthRequestModel(UserAuthRequestModel.FlowRateMessageAuthStatus.Completed);

                SendMessageToUser(userModel.Id, new BaseMessage(userModel.Id, MessageType.UserAuth, userAddedToSessionMessage));
            }
            return true;
        }
        
        internal async Task RemoveUserFromInstance(UserModel userModel)
        {
            await Task.Delay(500);
            ServerTools.Logger.LogInformation($"User {userModel.Id} detached.");
            foreach (var room in Sessions)
            {
                foreach (var peer in room.Value.Peers)
                {
                    if (peer.Value.UserId == userModel.Id)
                    {
                        room.Value.RemovePeer(peer.Value.Id);
                    }
                }
            }
            AssociatedSockets.Remove(userModel.Id);
            ConnectedUsers.Remove(userModel.Id);
        }
        
        public void ProcessEvent(BaseMessage message, IWebSocketConnection webSocket)
        {
            if (message.UserID == null)
            {
                return;
            }
            
            if (!ConnectedUsers.ContainsKey(message.UserID))
            {
                ServerTools.Logger.LogWarning($"User {message.UserID} dont exists in coordinator session.");
                webSocket.Close();
                return;
            }

            if (MessagesValidator.Validate(message) && _handlers.TryGetValue(message.Type, out var handler))
            {
                handler.HandleMessage(message);
                return;
            }
            
            switch (message.Type)
            {
                case MessageType.Undefiend:
                    ServerTools.Logger.LogWarning("Undefiend event received, something went wrong.");

                    break;

                case MessageType.UserDisconnection:
                    if (ConnectedUsers.TryGetValue(message.UserID, out var dissconectedUser))
                    {
                        //RemoveUserFromInstance(dissconectedUser);
                        ServerTools.Logger.LogWarning($"User with id {message.UserID} disconnected.");
                    }

                    break;

                default:
                    ServerTools.Logger.LogError("Cant read event data.");
                    break;

            }
        }
    }
}
