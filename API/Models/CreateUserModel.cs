﻿using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class CreateUserModel
    {
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }

        [Required]
        [Compare(nameof(Password))]
        public string? RetryPassword { get; set; }

        public CreateUserModel(string? name, string? email, string? password, string? retryPassword)
        {
            Name = name;
            Email = email;
            Password = password;
            RetryPassword = retryPassword;
        }
    }
}
