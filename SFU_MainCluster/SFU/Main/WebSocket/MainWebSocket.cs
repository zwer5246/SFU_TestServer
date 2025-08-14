using System.Diagnostics.CodeAnalysis;
using Fleck;
using MessagesModels.Enums;
using MessagesModels.Models;
using Newtonsoft.Json.Linq;
using SFU_MainCluster.SFU.Tools;

namespace SFU_MainCluster.SFU.Main.WebSocket
{
    [method: SetsRequiredMembers]
    public class MainWebSocket(Server.Coordinator.Coordinator coordinator, IWebSocketConnection socket)
    {
        private UserModel? User { get; set; }

        public Action<string> OnMessage => (e) =>
        {
            //logger.LogDebug($"OnMessage: {e.Data}");

            BaseMessage? message = ServerTools.DeSerializeMessage(e);

            //if (_user == null)
            //{
            //    if (message.Event == WebRTCMessageModel.FlowRateMessageEvents.UserAuth)
            //    {
            //        _peerClientConfiguration = (PeerClientConfigurationModel)message.Data;
            //        ServerTools.Logger.LogDebug("Configuration recived. Connection proceed...");
            //        _mainCoordinator.
            //        return;
            //    }
            //    else
            //    {
            //        ServerTools.Logger.LogError("Connection closed. Client configuration not initiated.");
            //        Context.WebSocket.Close();
            //        return;
            //    }
            //}
            //else
            if (User == null)
            {
                coordinator.ProcessEvent(message, socket);
                if (message.Type == MessageType.UserAuth)
                {
                    // Проверка пользователя

                    User = ((JObject)message.Data).ToObject<UserModel>();
                    ServerTools.Logger.LogDebug("User data received. Connection proceed...");

                    coordinator.AddUserToInstance(User, socket);
                }
                else
                {
                    ServerTools.Logger.LogError("Connection closed. User must be validated.");
                    socket.Close();
                }

                return;
            }
            coordinator.ProcessEvent(message, socket);
        };

        public Action OnOpen => () =>
        {
            ServerTools.Logger.LogInformation(
                "Web socket client connection from {UserEndPoint}, waiting client config...",
                socket.ConnectionInfo.ClientIpAddress);
        };

        public Action<Exception> OnError => (e) => 
        {
            ServerTools.Logger.LogError("Error on ws connection. " + e.Message);
        };

        public Action OnClose => () =>
        {
            if (User != null)
            {
                coordinator.RemoveUserFromInstance(User);
            }
        };
    }
}

