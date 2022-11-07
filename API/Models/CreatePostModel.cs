using DAL.Entities;

namespace API.Models
{
    public class CreatePostModel
    {
        public string? PostContent { get; set; }
        public ICollection<MetadataModel> PostAttachments { get; set; } = null!;
    }
}
