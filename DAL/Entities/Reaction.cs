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
        public Guid Id { get; set; }
        public User ReactionAuthor { get; set; } = null!;
        public Guid ReactionAuthorId { get; set; }

        public Post ReactionPost { get; set; } = null!;
        public Guid ReactionPostId { get; set; }

        public ReactionType ReactionType { get; set; }
    }
}
