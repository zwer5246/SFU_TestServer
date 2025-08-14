using Fleck;
using MessagesModels.Models;
using SIPSorcery.Net;

namespace SFU_MainCluster.SFU.Main.Server.Video
{
    public partial class Peer : PeerModel
    {
        public bool IsPcReady { get; private set; }
        public bool IsAudioRequested { get; private set; }
        public RTCPeerConnection? Pc { get; private set; } 
        private readonly Mutex _mutex;
        private IWebSocketConnection? WebSocket { get; set; }
        private MediaStreamTrack? AudioTrack { get; set; }
        private MediaStreamTrack? VideoTrack { get; set; }

        public IWebSocketConnection WebSocketInstance => WebSocket!;
    }
}

