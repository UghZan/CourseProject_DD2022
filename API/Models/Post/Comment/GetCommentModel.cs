using API.Models.User;

namespace API.Models.Post.Comment
{
    public class GetCommentModel
    {
        public string? PostContent { get; set; }
        public GetUserModelWithAvatar Author { get; set; } = null!;
        public DateTimeOffset CreationDate { get; set; }
        public int ReactionsCount { get; set; }
    }
}
