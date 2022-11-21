using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string? PostContent { get; set; }
        public Guid AuthorId { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public Guid ParentPostId { get; set; }

        public virtual Post ParentPost { get; set; } = null!;
        public virtual User Author { get; set; } = null!;
    }
}
