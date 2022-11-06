using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PasswordHashed { get; set; }
        public DateTimeOffset CreateDate { get; set; }

        public virtual ICollection<UserSession>? Sessions { get; set; }
    }
}
