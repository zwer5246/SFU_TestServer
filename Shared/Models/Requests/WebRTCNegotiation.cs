using System.Diagnostics.CodeAnalysis;
using MessagesModels.Enums;

namespace MessagesModels.Models.Requests
{
    public class WebRTCNegotiation
    {
        public WebRTCNegotiationType Type { get; set; }
        public required string PeerId { get; set; }
        public required string RoomId { get; set; }
        public object? Data { get; set; }
        public WebRTCNegotiationResult Result { get; set; }
        public WebRTCNegotiation()
        {

        }

        [SetsRequiredMembers]
        public WebRTCNegotiation(WebRTCNegotiationType type, string peerId, string roomId, object? data)
        {
            Type = type;
            PeerId = peerId;
            RoomId = roomId;
            Data = data;
        }
        
        [SetsRequiredMembers]
        public WebRTCNegotiation(WebRTCNegotiation request)
        {
            Type = request.Type;
            PeerId = request.PeerId;
            RoomId = request.RoomId;
            Data = request.Data;
        }
        
        [SetsRequiredMembers]
        public WebRTCNegotiation(WebRTCNegotiation request, WebRTCNegotiationResult result)
        {
            Type = request.Type;
            PeerId = request.PeerId;
            RoomId = request.RoomId;
            Data = request.Data;
            Result = result;
        }
    }
}
