using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class UserSession
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid RefreshTokenId { get; set; }
        public DateTimeOffset SessionCreatedTime { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual User UserOfThisSession { get; set; } = null!;
    }
}
