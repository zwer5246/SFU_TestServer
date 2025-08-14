using MessagesModels.Enums;
using MessagesModels.Models;
using SFU_MainCluster.SFU.Main.Server.Video;
using SFU_MainCluster.SFU.Tools;
using Peer = SFU_MainCluster.SFU.Main.Server.Video.Peer;

namespace SFU_MainCluster.SFU.Main.Server.Coordinator
{
    public partial class Coordinator
    {
        // TODO: Implement voice and chat conditions
        /// <summary>
        ///  Creates room from model
        /// </summary>
        /// <param name="roomModel"></param>
        /// <returns></returns>
        internal CreateRoomResult CreateRoom(RoomModel roomModel)
        {
            try
            {
                if (Enumerable.Any<KeyValuePair<string, VideoSession>>(Sessions, session => session.Value.Name == roomModel.Name))
                {
                    ServerTools.Logger.LogInformation("CreateRoom event error. Name already used");
                    return CreateRoomResult.NameAlreadyUsed;
                }

                if (roomModel.Capacity <= 1)
                {
                    ServerTools.Logger.LogInformation("CreateRoom event error. Wrong room capacity");
                    return CreateRoomResult.WrongCapacity;
                }
            
                Sessions.Add(roomModel.Id, new VideoSession(roomModel));

                ServerTools.Logger.LogInformation("Room with name \"{Name}\"(ID: {newRoomId}) created. Waiting for host...",  roomModel.Name, roomModel.Id);
                
                return CreateRoomResult.NoError;
            }
            catch (Exception e)
            {
                ServerTools.Logger.LogWarning("CreateRoom event error. Exception: {e}", e.Message);
                return CreateRoomResult.InternalError;
            }
        }
        
        // TODO: Implement voice and chat conditions
        /// <summary>
        ///  Creates room from parameters
        /// </summary>
        /// <param name="name"></param>
        /// <param name="hostId"></param>
        /// <param name="capacity"></param>
        /// <param name="isAudioRequested"></param>
        /// <returns></returns>
        internal CreateRoomResult CreateVideoSession(string name, string hostId, int capacity, bool isAudioRequested)
        {
            try
            {
                var newRoomId = Guid.NewGuid();
                if (Enumerable.Any<KeyValuePair<string, VideoSession>>(Sessions, session => session.Value.Name == name))
                {
                    ServerTools.Logger.LogInformation("CreateVideoSession event error. Name already used");
                    return CreateRoomResult.NameAlreadyUsed;
                }

                if (capacity <= 1)
                {
                    ServerTools.Logger.LogInformation("CreateVideoSession event error. Wrong room capacity");
                    return CreateRoomResult.WrongCapacity;
                }
            
                Sessions.Add(newRoomId.ToString(), new VideoSession(newRoomId.ToString(), name, hostId, capacity, isAudioRequested));

                ServerTools.Logger.LogInformation("Room with name \"{Name}\"(ID: {newRoomId}) created. Waiting for host...", name, newRoomId.ToString());
                
                return CreateRoomResult.NoError;
            }
            catch (Exception e)
            {
                ServerTools.Logger.LogWarning("CreateVideoSession event error. Exception: {e}", e.Message);
                return CreateRoomResult.InternalError;
            }
        }
        
        internal DeleteRoomResult DeleteRoom(string roomId)
        {
            try
            {
                if (Sessions.TryGetValue(roomId, out var session))
                {
                    if (session.Peers.Count != 0)
                    {
                        ServerTools.Logger.LogWarning("DeleteRoom event error. Room contains peers");
                        return DeleteRoomResult.InternalError;
                    }
                }
                
                ServerTools.Logger.LogWarning("DeleteRoom event error. Room not exists");
                return DeleteRoomResult.RoomNotExists;
            }
            catch (Exception e)
            {
                ServerTools.Logger.LogWarning("DeleteRoom event error. Exception: {e}.", e.Message);
                return DeleteRoomResult.InternalError;
            }
        }
        
        internal JoinRoomResult JoinRoom(string roomId, string userId)
        {
            try
            {
                if (Sessions.TryGetValue(roomId, out VideoSession? session))
                {
                    if (session.Capacity == session.Peers.Count)
                    {
                        ServerTools.Logger.LogWarning("JoinRoom event error. Room full.");
                        return JoinRoomResult.RoomFull;
                    }
                    var newPeerId = Guid.NewGuid();
                    var peer = new Peer(newPeerId.ToString(), userId, session.IsAudioRequested);
                    if (peer.UserId == session.HostId)
                    {
                        peer.IsStreamHost = true;
                    }

                    peer.SetWebSocket(AssociatedSockets[userId]);
                    session.AddPeer(peer);
                    
                    return JoinRoomResult.NoError;
                }

                ServerTools.Logger.LogWarning("JoinRoom event error. Room not exist.");
                return JoinRoomResult.RoomNotExists;
            }
            catch (Exception e)
            {
                ServerTools.Logger.LogWarning("JoinRoom event error. Exception: {e}.", e.Message);
                return JoinRoomResult.InternalError;
            }
        }
        
        internal LeaveRoomResult LeaveRoom(string roomId, string peerId)
        {
            try
            {
                if (Sessions.TryGetValue(roomId, out var session))
                {
                    if (session.RemovePeer(peerId))
                    {
                        ServerTools.Logger.LogWarning("LeaveRoom event error. Requested peer not exist.");
                        return LeaveRoomResult.InternalError;
                    }
                    return LeaveRoomResult.NoError;
                }

                ServerTools.Logger.LogWarning("LeaveRoom event error. Requested room not exist.");
                return LeaveRoomResult.InternalError;
            }
            catch (Exception e)
            {
                ServerTools.Logger.LogWarning("LeaveRoom event error. Exception: {e}.", e.Message);
                return LeaveRoomResult.InternalError;
            }
        }
    }
}
