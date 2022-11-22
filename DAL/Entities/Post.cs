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

        public virtual ICollection<PostPhoto> PostAttachments { get; set; } = null!;
        public virtual ICollection<Comment> PostComments { get; set; } = null!;
        public virtual ICollection<PostReaction> PostReactions { get; set; } = null!;

        public Guid AuthorId { get; set; }
        public virtual User Author { get; set; } = null!;

        public DateTimeOffset CreationDate { get; set; }
    }
}
