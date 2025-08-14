using MessagesModels;
using MessagesModels.Enums;
using MessagesModels.Models;
using MessagesModels.Models.Requests;
using Newtonsoft.Json.Linq;
using SFU_MainCluster.SFU.Main.Server.Coordinator.MessagesProcessing.Interfaces;
using SFU_MainCluster.SFU.Tools;

namespace SFU_MainCluster.SFU.Main.Server.Coordinator.MessagesProcessing.MessagesHandlers
{
    public class RoomActionsMessagesHandler(Coordinator coordinator) : IMessageHandler
    {
        public async Task<HandleMessageResult> HandleMessage(BaseMessage message)
        {
            var roomRequest = ((JObject)message.Data).ToObject<UserRoomRequestModel>();
            if (roomRequest == null)
                return HandleMessageResult.InternalError;

            try
            {
                return roomRequest.RequestType switch
                {
                    RoomRequestType.Create => HandleCreate(roomRequest, message),
                    RoomRequestType.Join => HandleJoin(roomRequest, message),
                    RoomRequestType.Leave => HandleLeave(roomRequest, message),
                    RoomRequestType.Delete => HandleDelete(roomRequest, message),
                    RoomRequestType.Reconfigure => throw new NotImplementedException(),
                    _ => HandleMessageResult.InternalError
                };
            }
            catch (Exception ex)
            {
                ServerTools.Logger.LogWarning("Error in RoomAction handler. Exception: {exception}", ex.Message);
                SendResponse(message.UserID!, roomRequest, UserRoomRequestModel.RequestRoomResult.InternalError);
                return HandleMessageResult.InternalError;
            }
        }
        
        private HandleMessageResult HandleCreate(UserRoomRequestModel request, BaseMessage message)
        {
            var result = coordinator.CreateVideoSession(request.RoomName!, message.UserID!,
                request.Capacity, request.isAudioRequested);

            if (result == CreateRoomResult.NoError)
            {
                SendResponse(message.UserID!, request, UserRoomRequestModel.RequestRoomResult.Completed);
                return HandleMessageResult.NoError;
            }
            else
            {
                SendResponse(message.UserID!, request, UserRoomRequestModel.RequestRoomResult.Completed, ErrorProvider.GetErrorDescription((CreateRoomResult)result));
                return HandleMessageResult.InternalError;
            }
        }

        private HandleMessageResult HandleJoin(UserRoomRequestModel request, BaseMessage message)
        {
            var result = coordinator.JoinRoom(request.RoomId!, message.UserID!);
                            
            if (result == JoinRoomResult.NoError)
            {
                SendResponse(message.UserID!, request, UserRoomRequestModel.RequestRoomResult.Completed);
                return HandleMessageResult.NoError;
            }
            else
            {
                SendResponse(message.UserID!, request, UserRoomRequestModel.RequestRoomResult.Completed, ErrorProvider.GetErrorDescription((JoinRoomResult)result));
                return HandleMessageResult.InternalError;
            }
        }

        private HandleMessageResult HandleLeave(UserRoomRequestModel request, BaseMessage message)
        {
            var result = coordinator.LeaveRoom(request.RoomId!, request.PeerId!);
                            
            if (result == LeaveRoomResult.NoError)
            {
                SendResponse(message.UserID!, request, UserRoomRequestModel.RequestRoomResult.Completed);
                return HandleMessageResult.NoError;
            }
            else
            {
                SendResponse(message.UserID!, request, UserRoomRequestModel.RequestRoomResult.Completed, ErrorProvider.GetErrorDescription((LeaveRoomResult)result));
                return HandleMessageResult.InternalError;
            }
        }

        private HandleMessageResult HandleDelete(UserRoomRequestModel request, BaseMessage message)
        {
            var result = coordinator.DeleteRoom(request.RoomId!);
                            
            if (result == DeleteRoomResult.NoError)
            {
                SendResponse(message.UserID!, request, UserRoomRequestModel.RequestRoomResult.Completed);
                return HandleMessageResult.NoError;
            }
            else
            {
                SendResponse(message.UserID!, request, UserRoomRequestModel.RequestRoomResult.Completed, ErrorProvider.GetErrorDescription((DeleteRoomResult)result));
                return HandleMessageResult.InternalError;
            }
        }
        
        private void SendResponse(string userId, UserRoomRequestModel request, UserRoomRequestModel.RequestRoomResult result, string? info = null)
        {
            var response = new UserRoomRequestModel(request, result) { Info = info };
            coordinator.SendMessageToUser(userId, new BaseMessage(userId, MessageType.RoomRequest, response));
        }
    }
}
