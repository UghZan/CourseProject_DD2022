namespace API.Models
{
    public class GetCommentModel
    {
        public string? PostContent { get; set; }
        public UserModelWithAvatar Author { get; set; }
        public DateTimeOffset CreationDate { get; set; }
    }
}
