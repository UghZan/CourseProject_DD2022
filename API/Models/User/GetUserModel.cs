using System.ComponentModel.DataAnnotations;

namespace API.Models.User
{
    public class GetUserModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTimeOffset CreateDate { get; set; }
        public int SubscribersCount { get; set; }
        public int SubscriptionsCount { get; set; }
    }

    public class GetUserModelWithAvatar : GetUserModel
    {
        public string? LinkToAvatar { get; set; }
    }
}
