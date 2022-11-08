namespace API.Models
{
    public class PostPhotoModel
    {
        public string URL { get; set; } = null!;
        public string MimeType { get; set; } = null!;
        public Guid AttachId { get; set; }
    }
}
