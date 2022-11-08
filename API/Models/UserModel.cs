using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class UserModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTimeOffset CreateDate { get; set; }
    }

    public class UserModelWithAvatar : UserModel
    {
        public string? LinkToAvatar { get; set; }
        public UserModelWithAvatar(UserModel model, Func<UserModel, string?>? linkGenerator)
        {
            Id = model.Id;
            Name = model.Name;
            Email = model.Email;
            CreateDate = model.CreateDate;
            LinkToAvatar = linkGenerator?.Invoke(model);
        }
    }
}
