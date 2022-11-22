using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public enum ReactionType
    {
        LIKE,
        DISLIKE,
        HEART,
        LAUGH,
        ANGRY,
        SAD,
        WOAH
    }

    public class Reaction
    {
        public int Id { get; set; }
        public virtual User ReactionAuthor { get; set; } = null!;
        public Guid ReactionAuthorId { get; set; }
        public DateTimeOffset CreationDate { get; set; }

        public ReactionType ReactionType { get; set; }
    }

    public class PostReaction : Reaction
    {
        public virtual Post ReactionPost { get; set; } = null!;
        public Guid ReactionPostId { get; set; }
    }

    public class CommentReaction : Reaction
    {
        public virtual Comment ReactionComment { get; set; } = null!;
        public Guid ReactionCommentId { get; set; }
    }
}
