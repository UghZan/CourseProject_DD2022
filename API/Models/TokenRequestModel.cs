namespace API.Models
{
    public class TokenRequestModel
    {
        public string Login { get; set; }
        public string Password { get; set; }

        public TokenRequestModel(string login, string password)
        {
            Login = login;
            Password = password;
        }
    }

    public class RefreshTokenRequestModel
    {
        public string Token { get; set; }

        public RefreshTokenRequestModel(string token)
        {
            Token = token;
        }
    }
}
