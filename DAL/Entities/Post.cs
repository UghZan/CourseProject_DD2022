using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Post
    {
        public Guid Id { get; set; }
        public string? PostContent { get; set; }
        public ICollection<PostPhoto> PostAttachments { get; set; } = null!;
        public ICollection<Comment> PostComments { get; set; } = null!;
        public ICollection<Reaction> PostReactions { get; set; } = null!;
        public User Author { get; set; } = null!;
        public Guid AuthorId { get; set; }
        public DateTimeOffset CreationDate { get; set; }
    }
}
