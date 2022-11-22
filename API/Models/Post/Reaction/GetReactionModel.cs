using API.Models.User;
using DAL.Entities;

namespace API.Models.Post.Reaction
{
    public class GetReactionModel
    {
        public GetUserModelWithAvatar ReactionAuthor { get; set; } = null!;
        public ReactionType ReactionType { get; set; }
    }
}
