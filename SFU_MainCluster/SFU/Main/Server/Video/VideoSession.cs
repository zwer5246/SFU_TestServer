using System.Diagnostics.CodeAnalysis;
using System.Net;
using MessagesModels.Enums;
using MessagesModels.Models;
using MessagesModels.Models.Requests;
using SFU_MainCluster.SFU.Tools;
using SIPSorcery.Net;

namespace SFU_MainCluster.SFU.Main.Server.Video
{
    public partial class VideoSession
    {
         [SetsRequiredMembers]
        public VideoSession (string id, string name, string hostId, int? capacity, bool isAudioRequested) 
        {
            Id = id;
            Name = name;
            Mutex = new Mutex(false);
            Peers = new Dictionary<string, Peer>();
            HostId = hostId;
            Capacity = capacity ?? 0;
            IsAudioRequested = isAudioRequested;
            HostStatusChanged += IsHostConnected =>
            {
                UserRoomRequestModel hostStatusChangedMessage;
                if (IsHostConnected)
                {
                    hostStatusChangedMessage =
                        new UserRoomRequestModel(this.Id, RoomType.Video, RoomRequestType.HostConnected, UserRoomRequestModel.RequestRoomResult.Completed)
                            {
                                isHostConnected = IsHostConnected
                            };
                }
                else
                {
                    hostStatusChangedMessage =
                        new UserRoomRequestModel(this.Id, RoomType.Video, RoomRequestType.HostDisconnected, UserRoomRequestModel.RequestRoomResult.Completed)
                            {
                                isHostConnected = IsHostConnected
                            };
                }

                this.BroadCastMessage(new BaseMessage(MessageType.RoomRequest, hostStatusChangedMessage), true);
            };
        }
        
        [SetsRequiredMembers]
        public VideoSession (RoomModel roomModel) 
        {
            Id = roomModel.Id;
            Name = roomModel.Name;
            Mutex = new Mutex(false);
            Peers = new Dictionary<string, Peer>();
            HostId = roomModel.HostId;
            Capacity = roomModel.Capacity;
            IsAudioRequested = roomModel.IsAudioRequested;
            HostStatusChanged += isHostConnected =>
            {
                UserRoomRequestModel hostStatusChangedMessage;
                if (isHostConnected)
                {
                    hostStatusChangedMessage =
                        new UserRoomRequestModel(this.Id, RoomType.Video, RoomRequestType.HostConnected, UserRoomRequestModel.RequestRoomResult.Completed)
                            {
                                isHostConnected = isHostConnected
                            };
                }
                else
                {
                    hostStatusChangedMessage =
                        new UserRoomRequestModel(this.Id, RoomType.Video, RoomRequestType.HostDisconnected, UserRoomRequestModel.RequestRoomResult.Completed)
                            {
                                isHostConnected = isHostConnected
                            };
                }

                this.BroadCastMessage(new BaseMessage(MessageType.RoomRequest, hostStatusChangedMessage), true);
            };
        }
        
        /// <summary>
        /// Convert Room to RoomModel
        /// </summary>
        /// <returns> RoomModel </returns>
        public RoomModel AsModel() =>
            new RoomModel
            {
                Id = this.Id,
                Name = this.Name,
                Capacity = this.Capacity,
                HostId = this.HostId,
                HostPeerId = this.HostPeerId,
                IsAudioRequested = this.IsAudioRequested
            };
        
        /// <summary>
        /// Adds peer to room
        /// </summary>
        /// <param name="peer"></param>
        public void AddPeer(Peer peer)
        {
            Mutex.WaitOne();

            try
            {
                if (Peers.ContainsKey(peer.Id))
                {
                    RemovePeer(peer.Id);
                    ServerTools.Logger.LogInformation("Removing existing peer in room.");
                }

                // User connected
                if (peer.IsStreamHost)
                {
                    Peers.Add(peer.Id, peer);
                    HostPeerId = peer.Id;
                    IsHostConnected = true;
                    ServerTools.Logger.LogInformation($"Host with ID {peer.Id} connected to Room {Name}({Id}).");
                }
                else
                {
                    Peers.Add(peer.Id, peer);
                    ServerTools.Logger.LogInformation($"Client with ID {peer.Id} connected to Room {Name}({Id}).");
                }
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }
        
        public bool RemovePeer(string peerId)
        {
            Mutex.WaitOne();

            try
            {
                if (Peers.TryGetValue(peerId, out var findedPeer))
                {
                    Peers[peerId].DestroyPeerConnection();
                    Peers.Remove(peerId);
                    if (findedPeer.IsStreamHost)
                    {
                        IsHostConnected = false;
                        ServerTools.Logger.LogInformation($"Host with ID {Id} disconnected from Room {Name}({this.Id}).");
                        return true;
                    }
                    ServerTools.Logger.LogInformation($"Client with ID {Id} disconnected from Room {Name}({this.Id}).");
                    return true;
                }

                return false;
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }
        
        // TODO: Add error answer
        /// <summary>
        ///  Sends answer to PeerConnection
        /// </summary>
        /// <param name="answer"></param>
        /// <param name="selfPeerId"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SendAnswer(RTCSessionDescriptionInit? answer, string selfPeerId)
        {
            if (selfPeerId == null)
            {
                throw new ArgumentNullException(nameof(selfPeerId));
            }
            
            if (Peers[selfPeerId].WebSocketInstance.IsAvailable)
            {
                WebRTCNegotiation answerMessage = new WebRTCNegotiation(WebRTCNegotiationType.Answer, selfPeerId, Id, answer);

                if (Peers.TryGetValue(selfPeerId, out var peer))
                {
                    if (peer.Pc != null)
                    {
                        peer.Pc.onconnectionstatechange += (state) =>
                        {
                            ServerTools.Logger.LogDebug($"Peer connection state change to {state}.");

                            if (state == RTCPeerConnectionState.failed)
                            {
                                ServerTools.Logger.LogInformation($"Peer connection failed.");
                            }
                            else if (state == RTCPeerConnectionState.connected)
                            {
                                HandleSendRtp(selfPeerId);
                            }
                            else if (state == RTCPeerConnectionState.closed)
                            {
                                UnHandleSendRtp(selfPeerId);
                            }
                        };
                        
                        Peers[selfPeerId].WebSocketInstance.Send(ServerTools.CreateAndSerializeMessage(answerMessage, MessageType.WebRTCInit));
                    }
                }
            }
        }
        
        // TODO: Add error answer
        /// <summary>
        ///  Sends answer to PeerConnection
        /// </summary>
        /// <param name="message"></param>
        /// <param name="selfPeerId"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SendOffer(BaseMessage message, string selfPeerId)
        {
            if (Peers[selfPeerId].WebSocketInstance.IsAvailable)
            {
                if (Peers.TryGetValue(selfPeerId, out var peer))
                {
                    if (peer.Pc != null)
                    {
                        peer.Pc.onconnectionstatechange += (state) =>
                        {
                            ServerTools.Logger.LogDebug($"Peer connection state change to {state}.");

                            if (state == RTCPeerConnectionState.failed)
                            {
                                ServerTools.Logger.LogInformation($"Peer connection failed.");
                            }
                            else if (state == RTCPeerConnectionState.connected)
                            {
                                HandleSendRtp(selfPeerId);
                            }
                            else if (state == RTCPeerConnectionState.closed)
                            {
                                UnHandleSendRtp(selfPeerId);
                            }
                        };
                    }
                }
                
                Peers[selfPeerId].WebSocketInstance.Send(ServerTools.SerializeMessage(message));
            }
        }
        
        private void BroadCastMessage(BaseMessage brMessage)
        {
            foreach (var peer in Peers)
            {
                peer.Value.WebSocketInstance.Send(ServerTools.SerializeMessage(brMessage));
            }
        }
        
        /// <summary>
        /// Broadcast message to all room members (exclude host)
        /// TODO: Remove host excluding
        /// </summary>
        /// <param name="brMessage"></param>
        /// <param name="excludeHost"></param>
        private void BroadCastMessage(BaseMessage brMessage, bool excludeHost)
        {
            foreach (var peer in Peers)
            {
                if (peer.Value.UserId != HostId)
                {
                    if (peer.Value.WebSocketInstance.IsAvailable)
                    {
                        peer.Value.WebSocketInstance.Send(ServerTools.SerializeMessage(brMessage));
                    }
                }
            }
        }
        
        private void HandleSendRtp(string peerId)
        {
            if (!Peers.TryGetValue(peerId, out var peer))
            {
                ServerTools.Logger.LogWarning($"Peer '{peerId}' not found.");
                return;
            }

            void RtpPacketHandler(IPEndPoint e, SDPMediaTypesEnum media, RTPPacket rtpPkt)
            {
                if (media != SDPMediaTypesEnum.audio && media != SDPMediaTypesEnum.video)
                    return;

                peer.Pc?.SendRtpRaw(
                    media,
                    rtpPkt.Payload,
                    rtpPkt.Header.Timestamp,
                    rtpPkt.Header.MarkerBit,
                    rtpPkt.Header.PayloadType
                );
            }

            if (_clientHandlers.TryAdd(peerId, RtpPacketHandler))
            {
                Peers[HostPeerId!].Pc!.OnRtpPacketReceived += _clientHandlers[peerId];
            }
            else
            {
                ServerTools.Logger.LogWarning($"Handler for peer '{peerId}' already exists.");
            }
        }

        
        private void UnHandleSendRtp(string peerId)
        {
            if (_clientHandlers.TryGetValue(peerId, out Action<IPEndPoint, SDPMediaTypesEnum, RTPPacket>? value))
            {
                if (Peers.TryGetValue(peerId, out var peer))
                {
                    if (peer.Pc != null)
                    {
                        peer.Pc.OnRtpPacketReceived -= value;
                    }
                }
                _clientHandlers.Remove(peerId);
            }
        }
    }
}
