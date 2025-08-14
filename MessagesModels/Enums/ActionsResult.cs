namespace MessagesModels.Enums
{
    public enum CreateRoomResult
    {
        InternalError = -1,
        NoError = 0,
        NameAlreadyUsed = 1,
        WrongCapacity = 2,
        UnexceptedParameters = 3
    }
    
    public enum DeleteRoomResult
    {
        InternalError = -1,
        NoError = 0,
        RoomContainsUsers = 1,
        RoomNotExists = 2
    }
        
    public enum JoinRoomResult
    {
        InternalError = -1,
        NoError = 0,
        RoomNotExists = 1,
        RoomFull = 2
    }
    
    public enum LeaveRoomResult
    {
        InternalError = -1,
        NoError = 0,
        RoomNotExists = 1
    }
    
    public enum GetRoomResult
    {
        InternalError = -1,
        NoError = 0,
        RoomNotExists = 1
    }
    
    public enum HandleMessageResult
    {
        InternalError = -1,
        NoError = 0,
        NotExceptedError = 1,
        ForbiddenMessage = 2
    }
    
    public enum ValidateMessageResult
    {
        NoError = 0,
        NotExceptedError = 1,
        NullDataReceived = 2,
        JsonParseError = 3,
        ServerAnswerReceived = 4,
        ForbiddenData = 5,
        CastError = 6
    }
    
    public enum WebRTCNegotiationResult
    {
        InternalError = -1,
        NoError = 0,
        NotExceptedError = 1
    }
}

