using MessagesModels.Enums;

namespace MessagesModels.Models
{
    public class RoomModel
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string HostId { get; set; }
        public required int Capacity { get; set; }
        public RoomType RoomType { get; set; }
        public string? HostPeerId { get; set; }
        protected bool IsHostConnected { get; set; }
        public bool IsAudioRequested;
    }
}
