using DAL.Entities;

namespace API.Models.Attach
{
    public class GetPostPhotoModel
    {
        public string? URL { get; set; }
        public string Name { get; set; } = null!;
        public string MimeType { get; set; } = null!;

        public GetPostPhotoModel(PostPhoto model, Func<Guid, string?>? linkGenerator)
        {
            Name = model.Name;
            MimeType = model.MimeType;
            URL = linkGenerator?.Invoke(model.Id);
        }
    }
}
