using Fleck;
using MessagesModels.Enums;
using MessagesModels.Models.Requests;
using SFU_MainCluster.SFU.Tools;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;

namespace SFU_MainCluster.SFU.Main.Server.Video
{
    public partial class Peer
    {
        public Peer(string id, string userId, bool isAudioRequested)
        {
            Id = id;
            UserId = userId;
            _mutex = new Mutex(false);
            IsAudioRequested = isAudioRequested;
            CreatePeerConnection(false);
        }
        
        public void SetWebSocket(IWebSocketConnection webSocket)
        {
            _mutex.WaitOne();

            WebSocket = webSocket;

            _mutex.ReleaseMutex();
        }

        /// <summary>
        /// Create WbeRTC peer connection
        /// </summary>
        /// <param name="isStreamHost"></param>
        public void CreatePeerConnection(bool isStreamHost)
        {
            _mutex.WaitOne();
            
            try
            {
                RTCConfiguration config = new RTCConfiguration
                {
                    iceServers = new List<RTCIceServer>
                    {
                        new RTCIceServer
                        {
                            urls = "stun:stun.sipsorcery.com"
                        }
                    }
                };
                Pc = new RTCPeerConnection(null);

                var sdpAudio = SDP.ParseSDPDescription(File.ReadAllText("ffmpeg_audio.sdp"));
                var audioAnn = sdpAudio.Media.Single(x => x.Media == SDPMediaTypesEnum.audio);
                SDPAudioVideoMediaFormat ffmpegAudioFormat = audioAnn.MediaFormats.Values.First();

                if (isStreamHost)
                {
                    VideoTrack = new MediaStreamTrack(
                        new VideoFormat(96, "H264", 90000, "profile-level-id=42e01f;packetization-mode=1;")
                    );
                    Pc.addTrack(VideoTrack);

                    AudioTrack = new MediaStreamTrack(SDPMediaTypesEnum.audio, false,
                        new List<SDPAudioVideoMediaFormat> { ffmpegAudioFormat });
                    Pc.addTrack(AudioTrack);
                }
                else
                {
                    VideoTrack = new MediaStreamTrack(
                        new VideoFormat(96, "H264", 90000, "profile-level-id=42e01f;packetization-mode=1;")
                    );
                    Pc.addTrack(VideoTrack);

                    AudioTrack = new MediaStreamTrack(SDPMediaTypesEnum.audio, false,
                        new List<SDPAudioVideoMediaFormat> { ffmpegAudioFormat });
                    Pc.addTrack(AudioTrack);
                }

                Pc.onicecandidate += async candidate =>
                {
                    await Task.Delay(3000);
                    WebRTCNegotiation iceMessage = new WebRTCNegotiation(WebRTCNegotiationType.ICE,
                        UserId, Id, candidate);
                    if (WebSocketInstance.IsAvailable)
                    {
                        await WebSocketInstance.Send(ServerTools.CreateAndSerializeMessage(iceMessage, MessageType.WebRTCInit));
                    }
                    else
                    {
                        Pc.close();
                    }
                };

                Pc.onnegotiationneeded += () =>
                {
                    ServerTools.Logger.LogDebug("Negotiation NEEDED!");
                };

                Pc.onconnectionstatechange += state =>
                {
                    ServerTools.Logger.LogDebug("Now RTCPeerConnection state {state}", state);
                };
                
                Pc.oniceconnectionstatechange += state =>
                {
                    ServerTools.Logger.LogDebug("Now RTCPeerConnection ICE state is {state}", state);
                };
                            
                Pc.onicegatheringstatechange += state =>
                {
                    ServerTools.Logger.LogDebug("Now RTCPeerConnection ICE gathering state is {state}", state);
                };
            }
            finally
            {
                IsPcReady = true;
                _mutex.ReleaseMutex();
            }
        }

        public void DestroyPeerConnection()
        {
            _mutex.WaitOne();
            
            try
            {
                Pc.Dispose();
            }
            finally
            {
                IsPcReady = false;
                _mutex.ReleaseMutex();
            }
            
        }
        
        /// <summary>
        /// Reacts on WebRTC answer
        /// </summary>
        /// <param name="answerDesc"></param>
        /// <returns></returns>
        public void ReactOnAnswer(RTCSessionDescriptionInit answerDesc)
        {
            _mutex.WaitOne();
            try
            {
                if (Pc != null)
                {
                    Pc!.setRemoteDescription(answerDesc);
                }
                else
                {
                    throw new ApplicationException("React on answer exception. PeerConnection not created.");
                }
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }
        
        /// <summary>
        /// Subject to change
        /// </summary>
        /// <param name="offerDesc"></param>
        public void ReactOnOffer(RTCSessionDescriptionInit? offerDesc)
        {
            _mutex.WaitOne();
            try
            {
                if (Pc != null && offerDesc != null)
                {
                    var result = Pc.setRemoteDescription(offerDesc);
                    if (result != SetDescriptionResultEnum.OK)
                    {
                        ServerTools.Logger.LogError($"Failed to set remote description, {result}.");
                        DestroyPeerConnection();
                    }
                }
                else
                {
                    throw new ApplicationException("React on offer exception. PeerConnection not created.");
                }
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }
        
        /// <summary>
        /// Reacts on WebRTC Offer Request
        /// TODO: -
        /// </summary>
        /// <returns></returns>
        public async Task<RTCSessionDescriptionInit?>? ReactOnOfferRequest()
        {
            _mutex.WaitOne();
            try
            {
                if (Pc != null)
                {
                    RTCSessionDescriptionInit? offerSdp = Pc!.createOffer();
                    await Pc.setLocalDescription(offerSdp);
                    return offerSdp;
                }
                else
                {
                    throw new ApplicationException("React on offer request exception. PeerConnection not created.");
                }
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Reacts on WebRTC ICE candidate
        /// </summary>
        /// <param name="candidate"></param>
        public void ReactOnICE(RTCIceCandidateInit candidate)
        {
            _mutex.WaitOne();
            try
            {
                if (Pc != null)
                {
                    Pc.addIceCandidate(candidate);
                }
                else
                {
                    throw new ApplicationException("React on ICE candidate exception. PeerConnection not created.");
                }
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }
    }
}
