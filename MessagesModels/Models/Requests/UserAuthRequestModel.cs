namespace MessagesModels.Models.Requests
{
    public class UserAuthRequestModel
    {
        public FlowRateMessageAuthStatus status;
        public List<FlowRateMessageAuthStatus>? specialTags;
        public string? AuthMessage;

        public enum FlowRateMessageAuthStatus
        {
            WrongLoginData = 0,
            Banned = 1,
            Suspended = 2,
            Completed = 3
        }

        public enum FlowRateMessageAuthTag
        {
            Reconnected = 0
        }

        public UserAuthRequestModel()
        {

        }

        public UserAuthRequestModel(FlowRateMessageAuthStatus AuthStatus)
        {
            this.status = AuthStatus;
        }

        public UserAuthRequestModel(FlowRateMessageAuthStatus AuthStatus, string Data)
        {
            this.status = AuthStatus;
            this.AuthMessage = Data;
        }
    }
}
