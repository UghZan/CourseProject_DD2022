namespace API.Models
{
    public class TokenRequestModel
    {
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;

        public TokenRequestModel(string login, string password)
        {
            Login = login;
            Password = password;
        }
    }

    public class RefreshTokenRequestModel
    {
        public string Token { get; set; } = null!;

        public RefreshTokenRequestModel(string token)
        {
            Token = token;
        }
    }
}
