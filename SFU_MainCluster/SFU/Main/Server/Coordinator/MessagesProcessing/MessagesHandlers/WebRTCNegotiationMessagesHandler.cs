using MessagesModels.Enums;
using MessagesModels.Models;
using MessagesModels.Models.Requests;
using Newtonsoft.Json.Linq;
using SFU_MainCluster.SFU.Main.Server.Coordinator.MessagesProcessing.Interfaces;
using SFU_MainCluster.SFU.Tools;
using SIPSorcery.Net;

namespace SFU_MainCluster.SFU.Main.Server.Coordinator.MessagesProcessing.MessagesHandlers
{
    public class WebRTCNegotiationMessagesHandler(Coordinator coordinator) : IMessageHandler
    {
        public async Task<HandleMessageResult> HandleMessage(BaseMessage message)
        {
            var negotiationRequest = ((JObject)message.Data).ToObject<WebRTCNegotiation>()!;
            try
            {
                return negotiationRequest.Type switch
                {
                    WebRTCNegotiationType.Offer =>  HandleOffer(negotiationRequest, message),
                    WebRTCNegotiationType.OfferRequest => await HandleOfferRequest(negotiationRequest, message),
                    WebRTCNegotiationType.Answer =>  HandleAnswer(negotiationRequest, message),
                    WebRTCNegotiationType.ICE =>  HandleICE(negotiationRequest, message),
                    _ => HandleMessageResult.InternalError
                };
            }
            catch (Exception ex)
            {
                ServerTools.Logger.LogWarning("Error in RoomAction handler. Exception: {exception}", ex.Message);
                SendResponse(message.UserID!, negotiationRequest, WebRTCNegotiationResult.InternalError);
                return HandleMessageResult.InternalError;
            }
        }

        //  TODO: PeerConnection dereference of a possibly null reference
        /// <summary>
        ///  Handle offer
        /// </summary>
        /// <param name="request"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private HandleMessageResult HandleOffer(WebRTCNegotiation request, BaseMessage message)
        {
            ServerTools.Logger.LogDebug("ReactOnOffer event initiated.");
            var sdpOffer = ((JObject)request.Data!).ToObject<RTCSessionDescriptionInit>();
            if (coordinator.Sessions.TryGetValue(request.RoomId, out var session ))
            {
                if (session.Peers.TryGetValue(request.PeerId, out var peer))
                {
                    peer.ReactOnOffer(sdpOffer);
                    var sdpAnswer = peer.Pc.createAnswer();
                    session.SendAnswer(sdpAnswer, request.PeerId);
                    ServerTools.Logger.LogInformation("ReactOnOffer event completed.");
                    return HandleMessageResult.NoError;
                }
                else
                {
                    ServerTools.Logger.LogWarning("ReactOnOffer event error. Requested peer not exist.");
                    return HandleMessageResult.InternalError;
                }
            }
            else
            {
                ServerTools.Logger.LogWarning("ReactOnOffer event error. Requested room not exist.");
                return HandleMessageResult.InternalError;
            }
        }

        private async Task<HandleMessageResult> HandleOfferRequest(WebRTCNegotiation request, BaseMessage message)
        {
            ServerTools.Logger.LogDebug("ReactOnOfferRequest event initiated.");
            if (coordinator.Sessions.TryGetValue(request.RoomId, out var session))
            {
                if (session.Peers.TryGetValue(request.PeerId, out var peer))
                {
                    peer.CreatePeerConnection(false);
                    var offerSdp = new WebRTCNegotiation(
                        WebRTCNegotiationType.OfferRequest,
                        request.PeerId, request.RoomId,
                        await peer.ReactOnOfferRequest()!);
                    var offerMessage = new BaseMessage(MessageType.WebRTCInit, offerSdp);
                    session.SendOffer(offerMessage, request.PeerId);
                    return HandleMessageResult.NoError;
                }
                else
                {
                    ServerTools.Logger.LogWarning("ReactOnOfferRequest event error. Requested peer not exist.");
                    return HandleMessageResult.InternalError;
                }
            }
            else
            {
                ServerTools.Logger.LogWarning("ReactOnOfferRequest event error. Requested room not exist.");
                return HandleMessageResult.InternalError;
            }
        }

        private HandleMessageResult HandleAnswer(WebRTCNegotiation request, BaseMessage message)
        {
            ServerTools.Logger.LogDebug("ReactOnAnswer event initiated.");
            var sdpAnswer = ((JObject)request.Data).ToObject<RTCSessionDescriptionInit>();
            if (coordinator.Sessions.TryGetValue(request.RoomId, out var session))
            {
                if (session.Peers.TryGetValue(request.PeerId, out var peer))
                {
                    peer.ReactOnAnswer(sdpAnswer);
                    ServerTools.Logger.LogInformation("ReactOnAnswer event completed.");
                    return HandleMessageResult.NoError;
                }
                else
                {
                    ServerTools.Logger.LogWarning("ReactOnAnswer event error. Requested peer not exist.");
                    return HandleMessageResult.InternalError;
                }
            }
            else
            {
                ServerTools.Logger.LogWarning("ReactOnAnswer event error. Requested room not exist.");
                return HandleMessageResult.InternalError;
            }
        }

        private HandleMessageResult HandleICE(WebRTCNegotiation request, BaseMessage message)
        {
            ServerTools.Logger.LogDebug("ReactOnICE event initiated.");
            RTCIceCandidateInit candidate = ((JObject)request.Data).ToObject<RTCIceCandidateInit>();
            if (coordinator.Sessions.TryGetValue(request.RoomId, out var session))
            {
                if (session.Peers.TryGetValue(request.PeerId, out var peer))
                {
                    peer.ReactOnICE(candidate);
                    ServerTools.Logger.LogDebug("ReactOnICE event completed.");
                    return HandleMessageResult.NoError;
                }
                else
                {
                    ServerTools.Logger.LogWarning("ReactOnICE event error. Requested peer not exist.");
                    return HandleMessageResult.InternalError;
                }
            }
            else
            {
                ServerTools.Logger.LogWarning("ReactOnICE event error. Requested room not exist.");
                return HandleMessageResult.InternalError;
            }
        }

        private void SendResponse(string userId, WebRTCNegotiation request, WebRTCNegotiationResult result, string? info = null)
        {
            var response = new WebRTCNegotiation(request, result);
            coordinator.SendMessageToUser(userId, new BaseMessage(userId, MessageType.RoomRequest, response));
        }
    }
}

