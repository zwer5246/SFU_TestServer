using System.Diagnostics.CodeAnalysis;
using MessagesModels.Enums;

namespace MessagesModels.Models.Requests
{
    public class UserRoomRequestModel
    {
        public string? RoomId { get; set; }
        public string? RoomName { get; set; }
        public string? PeerId { get; set; }
        public int Capacity { get; set; }
        public RoomType RoomType { get; set; }
        public RoomRequestType RequestType { get; set; }
        public RequestRoomResult? Result { get; set; }
        public string? Info { get; set; }
        public bool? isHostConnected { get; set; }
        public bool isAudioRequested { get; set; }

        public enum RequestRoomResult
        {
            InternalError = 0,
            Uncompleted = 1,
            Completed = 4
        }

        public UserRoomRequestModel()
        {

        }
        
        [SetsRequiredMembers]
        public UserRoomRequestModel(string roomName, RoomType roomType, RoomRequestType requestType, ushort capacity)
        {
            RoomName = roomName;
            RequestType = requestType;
            Capacity = capacity;
            RoomType = roomType;
        }
        
        [SetsRequiredMembers]
        public UserRoomRequestModel(string roomId, RoomType roomType, RoomRequestType requestType)
        {
            RoomId = roomId;
            RequestType = requestType;
            RoomType = roomType;
        }
        
        [SetsRequiredMembers]
        public UserRoomRequestModel(string roomId, RoomType roomType, RoomRequestType requestType, RequestRoomResult result)
        {
            RoomId = roomId;
            RequestType = requestType;
            Result = result;
            RoomType = roomType;
        }
        
        [SetsRequiredMembers]
        public UserRoomRequestModel(UserRoomRequestModel request)
        {
            RoomId = request.RoomId;
            RequestType = request.RequestType;
            RoomType = request.RoomType;
        }
        
        [SetsRequiredMembers]
        public UserRoomRequestModel(UserRoomRequestModel request, RequestRoomResult result)
        {
            RoomId = request.RoomId;
            RequestType = request.RequestType;
            Result = result;
            RoomType = request.RoomType;
        }
    }
}
