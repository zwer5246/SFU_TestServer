namespace MessagesModels.Models
{
    public class PeerModel
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public bool IsStreamHost { get; set; }
    }
}
