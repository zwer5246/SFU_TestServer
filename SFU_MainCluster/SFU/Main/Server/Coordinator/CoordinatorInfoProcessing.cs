using MessagesModels.Enums;
using MessagesModels.Models;
using SFU_MainCluster.SFU.Tools;

namespace SFU_MainCluster.SFU.Main.Server.Coordinator
{
    public partial class Coordinator
    {
        internal GetRoomResult GetAllSessions(out List<RoomModel> rooms)
        {
            rooms = new List<RoomModel>();
            foreach (var room in Sessions)
            {
                rooms.Add(room.Value.AsModel());
            }
            return GetRoomResult.NoError;
        }
        
        internal GetRoomResult GetSessionInfo(string roomId, out RoomModel? findedSession)
        {
            try
            {
                if (Sessions.TryGetValue(roomId, out var room))
                {
                    findedSession = room.AsModel();
                    return GetRoomResult.NoError;
                }
                else
                {
                    ServerTools.Logger.LogDebug($"Error in GetSessionInfo. Requested room ({roomId}) not exists.");
                    findedSession = null;
                    return GetRoomResult.RoomNotExists;
                }
            }
            catch (Exception e)
            {
                ServerTools.Logger.LogWarning("GetSessionInfo event error. Exception: {e}.", e.Message);
                findedSession = null;
                return GetRoomResult.InternalError;
            }
        }
    }
}
