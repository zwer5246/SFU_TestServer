using MessagesModels.Enums;
using MessagesModels.Models;
using MessagesModels.Models.Requests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SFU_MainCluster.SFU.Tools;
using SIPSorcery.Net;

namespace SFU_MainCluster.SFU.Main.Server.Coordinator.MessagesProcessing.MessagesValidator
{
    public static class MessagesValidator
    {
        public static bool Validate(BaseMessage message)
        {
            try
            {
                ValidateMessageResult result = message.Type switch
                {
                    MessageType.WebRTCInit => ValidateFields(((JObject)message.Data).ToObject<WebRTCNegotiation>()),
                    MessageType.RoomRequest => ValidateFields(((JObject)message.Data).ToObject<UserRoomRequestModel>()),
                    _ => ValidateMessageResult.ForbiddenData
                };

                if (result == ValidateMessageResult.NoError)
                {
                    return true;
                }

                return false;
            }
            catch (JsonException jsonE)
            {
                ServerTools.Logger.LogDebug("Message identify error. Json parse failed. Exception: {Field}", jsonE.Message);
                return false;
            }
        }
        
        private static ValidateMessageResult ValidateFields(UserRoomRequestModel? request)
        {
            ValidateMessageResult LogAndReturn(string field, ValidateMessageResult result)
            {
                var label = result switch
                {
                    ValidateMessageResult.NullDataReceived => "Null",
                    ValidateMessageResult.ForbiddenData => "Forbidden",
                    _ => "Unknown"
                };

                ServerTools.Logger.LogDebug("Room request verifying error. {Label} data received ({Field}).", label, field);
                return result;
            }
            
            try
            {
                if (request == null)
                    return LogAndReturn("Entire message", ValidateMessageResult.NullDataReceived);

                if (request.RequestType == null)
                    return LogAndReturn("Type", ValidateMessageResult.NullDataReceived);

                if (!Enum.IsDefined(typeof(RoomRequestType), request.RequestType))
                    return LogAndReturn("Type", ValidateMessageResult.ForbiddenData);

                switch (request.RequestType)
                {
                    case RoomRequestType.Create:
                        if (string.IsNullOrWhiteSpace(request.RoomName))
                            return LogAndReturn("RoomName", ValidateMessageResult.NullDataReceived);

                        if (request.Capacity == null)
                            return LogAndReturn("Capacity", ValidateMessageResult.NullDataReceived);

                        if (request.Capacity <= 1)
                            return LogAndReturn("Capacity", ValidateMessageResult.ForbiddenData);
                        break;

                    case RoomRequestType.Join:
                        if (request.RoomId == null)
                            return LogAndReturn("RoomID", ValidateMessageResult.NullDataReceived);
                        break;

                    default:
                        if (request.RoomId == null)
                            return LogAndReturn("RoomID", ValidateMessageResult.NullDataReceived);

                        if (request.PeerId == null)
                            return LogAndReturn("PeerID", ValidateMessageResult.NullDataReceived);
                        break;
                }
                
                return ValidateMessageResult.NoError;
            }
            catch (Exception e)
            {
                ServerTools.Logger.LogDebug("Room request verifying error. Exception: {Field}", e.Message);
                return ValidateMessageResult.NotExceptedError;
            }
        }

        private static ValidateMessageResult ValidateFields(WebRTCNegotiation? request)
        {
                ValidateMessageResult LogAndReturn(string field, ValidateMessageResult result)
            {
                var label = result switch
                {
                    ValidateMessageResult.NullDataReceived => "Null",
                    ValidateMessageResult.ForbiddenData => "Forbidden",
                    _ => "Unknown"
                };

                ServerTools.Logger.LogDebug("WebRTC negotiation request verifying error. {Label} data received ({Field}).", label, field);
                return result;
            }

            try
            {
                if (request == null)
                    return LogAndReturn("Entire message", ValidateMessageResult.NullDataReceived);

                if (request.PeerId == null)
                    return LogAndReturn("PeerID", ValidateMessageResult.NullDataReceived);

                if (request.RoomId == null)
                    return LogAndReturn("RoomID", ValidateMessageResult.NullDataReceived);

                if (!Enum.IsDefined(typeof(WebRTCNegotiationType), request.Type))
                    return LogAndReturn("Type", ValidateMessageResult.ForbiddenData);

                switch (request.Type)
                {
                    case WebRTCNegotiationType.Answer:
                    case WebRTCNegotiationType.Offer:
                        if (request.Data == null)
                            return LogAndReturn("Data", ValidateMessageResult.NullDataReceived);
                        RTCSessionDescriptionInit? description = ((JObject)request.Data!).ToObject<RTCSessionDescriptionInit>();
                        break;
                    
                    case WebRTCNegotiationType.ICE:
                        if (request.Data == null)
                            return LogAndReturn("Data", ValidateMessageResult.NullDataReceived);
                        var candidate = ((JObject)request.Data!).ToObject<RTCIceCandidateInit>();
                        break;
                    
                    case WebRTCNegotiationType.OfferRequest:
                        break;
                        
                    default:
                        return LogAndReturn("Type", ValidateMessageResult.ForbiddenData);
                }

                return ValidateMessageResult.NoError;
            }
            catch (InvalidCastException castE)
            {
                ServerTools.Logger.LogDebug("WebRTC negotiation verifying error. Json exception: {Field}", castE.Message);
                return ValidateMessageResult.JsonParseError;
            }
            catch (Exception e)
            {
                ServerTools.Logger.LogDebug("WebRTC negotiation verifying error. Exception: {Field}", e.Message);
                return ValidateMessageResult.NotExceptedError;
            }
        }
    }
}

