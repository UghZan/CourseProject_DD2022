namespace API.Models
{
    public class GetCommentModel
    {
        public string? PostContent { get; set; }
        public Guid AuthorId { get; set; }
        public DateTimeOffset CreationDate { get; set; }
    }
}
