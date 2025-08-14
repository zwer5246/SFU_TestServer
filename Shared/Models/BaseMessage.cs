using MessagesModels.Enums;

namespace MessagesModels.Models
{
    public class BaseMessage
    {
        public string MessageID { get; set; }
        public string? UserID { get; set; }
        public MessageType Type { get; set; }
        public object Data { get; set; }

        public BaseMessage()
        {  
            
        }

        public BaseMessage(MessageType type, object Data)
        {
            this.MessageID = GenerateTimestampId();
            this.Type = type;
            this.Data = Data;
        }

        public BaseMessage(string UserID, MessageType type, object Data) 
        {
            this.MessageID = GenerateTimestampId();
            this.UserID = UserID;
            this.Type = type;
            this.Data = Data;
        }
        
        public static string GenerateTimestampId()
        {
            return $"{DateTime.UtcNow.Ticks}_{Guid.NewGuid().ToString().Substring(0, 8)}";
        }
    }
}
