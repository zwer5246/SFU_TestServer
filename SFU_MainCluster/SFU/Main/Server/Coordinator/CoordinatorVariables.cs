using Fleck;
using MessagesModels.Enums;
using MessagesModels.Models;
using SFU_MainCluster.SFU.Main.Server.Coordinator.MessagesProcessing.Interfaces;
using SFU_MainCluster.SFU.Main.Server.Video;

namespace SFU_MainCluster.SFU.Main.Server.Coordinator
{
    public partial class Coordinator
    {
        // Coordinator dictionaries
        public Dictionary<string, VideoSession> Sessions { get; set; }
        public Dictionary<string, UserModel?> ConnectedUsers { get; set; }
        public Dictionary<string, IWebSocketConnection> AssociatedSockets { get; set; }
        private readonly Dictionary<MessageType, IMessageHandler> _handlers = new();
    }
}
