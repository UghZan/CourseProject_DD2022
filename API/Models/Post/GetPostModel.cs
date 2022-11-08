using API.Models.Attach;
using API.Models.User;
using DAL.Entities;

namespace API.Models.Post
{
    public class GetPostModel
    {
        public string? PostContent { get; set; }
        public ICollection<GetPostPhotoModel> PostAttachments { get; set; } = null!;
        public GetUserModelWithAvatar Author { get; set; } = null!;
        public DateTimeOffset CreationDate { get; set; }
    }
}
