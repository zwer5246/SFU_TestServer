using MessagesModels.Enums;
using MessagesModels.Models;

namespace SFU_MainCluster.SFU.Main.Server.Coordinator.MessagesProcessing.Interfaces
{
    public interface IMessageHandler
    {
        Task<HandleMessageResult> HandleMessage(BaseMessage message);
    }
}
