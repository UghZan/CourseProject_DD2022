using API.Models.Attach;
using DAL.Entities;

namespace API.Models.Post
{
    public class CreatePostModel
    {
        public string? PostContent { get; set; }
        public ICollection<MetadataModel>? PostAttachments { get; set; }
    }
}
