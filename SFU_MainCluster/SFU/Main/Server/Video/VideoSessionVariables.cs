using System.Net;
using MessagesModels.Models;
using SIPSorcery.Net;

namespace SFU_MainCluster.SFU.Main.Server.Video
{
    public partial class VideoSession
    {
        public required string Id { get; init; }
        public required string Name { get; set; }
        public required string HostId { get; init; }
        public required int Capacity { get; set; }
        private bool isHostConnected { get; set; }
        private string? HostPeerId { get; set; }
        public bool IsAudioRequested { get; private set; }
        public required Mutex Mutex { get; set; }
        private Dictionary<string, Action<IPEndPoint, SDPMediaTypesEnum, RTPPacket>> _clientHandlers = new();
        private List<UserModel> AllowedUsers { get; set; } = [];
        public required Dictionary<string, Peer> Peers { get; init; }
        public event Action<bool> HostStatusChanged;
        public bool IsHostConnected
        {
            get => isHostConnected;
            set
            {
                if (isHostConnected != value)
                {
                    isHostConnected = value;
                    HostStatusChanged?.Invoke(value);
                }
            }
        }
    }
}
