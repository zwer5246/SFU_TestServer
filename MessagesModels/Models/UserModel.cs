
using System.Diagnostics.CodeAnalysis;

namespace MessagesModels.Models
{
    public class UserModel
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? NickName { get; set; }

        public UserModel()
        {

        }

        [SetsRequiredMembers]
        public UserModel (string id, string name, string nickName)
        {
            Id = id;
            Name = name;
            NickName = nickName;
        }

        [SetsRequiredMembers]
        public UserModel(string name, string nickName)
        {
            Name = name;
            NickName = nickName;
        }
    }
}
