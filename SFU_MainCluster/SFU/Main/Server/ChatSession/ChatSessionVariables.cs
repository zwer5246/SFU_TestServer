using MessagesModels.Models;

namespace SFU_MainCluster.SFU.Main.Server.ChatSession;

public partial class ChatSession
{
    public required string Id { get; init; }
    public required string Name { get; set; }
    public int? Capacity { get; set; }
    public required Mutex Mutex { get; set; }
    private List<UserModel> AllowedUsers { get; set; } = [];
}