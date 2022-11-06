using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class UserModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public DateTimeOffset CreateDate { get; set; }

        public UserModel(Guid id, string? name, string? email, DateTimeOffset createDate)
        {
            Id = id;
            Name = name;
            Email = email;
            CreateDate = createDate;
        }
    }
}
