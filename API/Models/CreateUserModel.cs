using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class CreateUserModel
    {
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;

        [Required]
        [Compare(nameof(Password))]
        public string? RetryPassword { get; set; } = null!;

        public CreateUserModel(string name, string email, string password, string retryPassword)
        {
            Name = name;
            Email = email;
            Password = password;
            RetryPassword = retryPassword;
        }
    }
}
