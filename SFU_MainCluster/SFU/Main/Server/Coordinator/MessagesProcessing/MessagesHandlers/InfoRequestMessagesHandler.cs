using MessagesModels.Enums;
using MessagesModels.Models;
using MessagesModels.Models.Requests;
using Newtonsoft.Json.Linq;
using SFU_MainCluster.SFU.Main.Server.Coordinator.MessagesProcessing.Interfaces;

namespace SFU_MainCluster.SFU.Main.Server.Coordinator.MessagesProcessing.MessagesHandlers
{
    public class InfoRequestMessagesHandler(Coordinator coordinator) : IMessageHandler
    {
        public async Task<HandleMessageResult> HandleMessage(BaseMessage message)
        {
            var infoRequest = ((JObject)message.Data).ToObject<UserRoomRequestModel>();
            switch (infoRequest!.RequestType)
            {
                case RoomRequestType.SessionInfo:
                    Task.Run(() =>
                    {
                        var result = coordinator.GetAllSessions(out var rooms);

                        if (result == GetRoomResult.NoError)
                        {
                            
                        }
                        else
                        {
                            
                        }
                    });
                    break;

                case RoomRequestType.SessionsInfo:
                    Task.Run(() =>
                    {
                        
                    });
                    break;

                default:
                    
                    break;
            }

            return HandleMessageResult.NoError;
        }
    }
}
