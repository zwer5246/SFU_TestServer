using MessagesModels.Enums;

namespace MessagesModels;

public class ErrorProvider
{
    public static string GetErrorDescription(JoinRoomResult result)
    {
        if (result == JoinRoomResult.NoError)
        {
            return "Request successful completed";
        }

        if (result == JoinRoomResult.InternalError)
        {
            return "Internal server error";
        }

        if (result == JoinRoomResult.RoomFull)
        {
            return "Room is full";
        }

        if (result == JoinRoomResult.RoomNotExists)
        {
            return "Room does not exist";
        }

        throw new ArgumentOutOfRangeException(nameof(result), result, "Unknown result");
    }
    
    public static string GetErrorDescription(CreateRoomResult result)
    {
        if (result == CreateRoomResult.NoError)
        {
            return "Request successful completed";
        }

        if (result == CreateRoomResult.InternalError)
        {
            return "Internal server error";
        }

        if (result == CreateRoomResult.WrongCapacity)
        {
            return "Wrong room capacity";
        }

        if (result == CreateRoomResult.NameAlreadyUsed)
        {
            return "New name already used";
        }

        throw new ArgumentOutOfRangeException(nameof(result), result, "Unknown result");
    }
    
    public static string GetErrorDescription(LeaveRoomResult result)
    {
        if (result == LeaveRoomResult.NoError)
        {
            return "Request successful completed";
        }

        if (result == LeaveRoomResult.InternalError)
        {
            return "Internal server error";
        }

        throw new ArgumentOutOfRangeException(nameof(result), result, "Unknown result");
    }
    
    public static string GetErrorDescription(DeleteRoomResult result)
    {
        if (result == DeleteRoomResult.NoError)
        {
            return "Request successful completed";
        }

        if (result == DeleteRoomResult.InternalError)
        {
            return "Internal server error";
        }
        
        if (result == DeleteRoomResult.RoomContainsUsers)
        {
            return "Room contains users";
        }
        
        if (result == DeleteRoomResult.RoomNotExists)
        {
            return "Room not exists";
        }

        throw new ArgumentOutOfRangeException(nameof(result), result, "Unknown result");
    }
    
    public static string GetErrorDescription(GetRoomResult result)
    {
        if (result == GetRoomResult.NoError)
        {
            return "Request successful completed";
        }

        if (result == GetRoomResult.InternalError)
        {
            return "Internal server error";
        }

        if (result == GetRoomResult.RoomNotExists)
        {
            return "Room does not exist";
        }

        throw new ArgumentOutOfRangeException(nameof(result), result, "Unknown result");
    }
}