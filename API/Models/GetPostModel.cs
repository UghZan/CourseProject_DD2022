using DAL.Entities;

namespace API.Models
{
    public class GetPostModel
    {
        public string? PostContent { get; set; }
        public ICollection<GetPostPhotoModel> PostAttachments { get; set; } = null!;
        public UserModelWithAvatar Author { get; set; }
        public DateTimeOffset CreationDate { get; set; }
    }
}
