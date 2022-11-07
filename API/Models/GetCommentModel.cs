namespace API.Models
{
    public class GetCommentModel : GetPostModel
    {
        public Guid PostId { get; set; }
    }
}
