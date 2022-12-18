using DAL.Entities;

namespace API.Models.Attach
{
    public class GetPostPhotoModel
    {
        public Guid Id { get; set; }
        public string? URL { get; set; }
        public string Name { get; set; } = null!;
        public string MimeType { get; set; } = null!;
    }
}
