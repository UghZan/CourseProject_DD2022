using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API.Configs
{
    public class AuthConfig
    {
        public const string SectionName = "AuthConfig";
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public int Lifetime { get; set; }
        public SymmetricSecurityKey SymmetricSecurityKey() => new(Encoding.UTF8.GetBytes(Key));

    }
}
