using API.Models.Attach;
using API.Models.User;
using DAL.Entities;

namespace API.Models.Post
{
    public class GetPostModel : GetPostContentModel
    {
        public Guid Id { get; set; }
        public GetUserModelWithAvatar Author { get; set; } = null!;
        public DateTimeOffset CreationDate { get; set; }
        public int ReactionsCount { get; set; }
        public int CommentsCount { get; set; }
    }

    public class GetPostContentModel
    {
        public string? PostContent { get; set; }
        public ICollection<GetPostPhotoModel> PostAttachments { get; set; } = null!;
    }
}
