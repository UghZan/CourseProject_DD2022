using API.Models.User;
using DAL.Entities;

namespace API.Models.Post.Reaction
{
    public class GetReactionModel
    {
        public Guid Id { get; set; }
        public Guid ReactionPostId { get; set; } //I think giving only a postID instead of the entire post is better as we probably don't look at specific reaction outside of it's parent post
        public GetUserModelWithAvatar ReactionAuthor { get; set; } = null!;
        public ReactionType ReactionType { get; set; }
    }
}
