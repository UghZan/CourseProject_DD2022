using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Comment : Post
    {
        public virtual Post ParentPost { get; set; } = null!;
        public Guid ParentPostId { get; set; }
    }
}
