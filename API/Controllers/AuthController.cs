using API.Models.Token;
using API.Models.User;
using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly UserService _userService;

        public AuthController(AuthService authService, UserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpPost]
        public async Task<TokenModel> Token(TokenRequestModel requestModel) => await _authService.GetToken(requestModel.Login, requestModel.Password);

        [HttpPost]
        public async Task<TokenModel> RefreshToken(RefreshTokenRequestModel reqModel) => await _authService.GetTokenByRefreshToken(reqModel.Token);


        [HttpPost]
        public async Task<Guid> RegisterUser(CreateUserModel model)
        {
            if (await _userService.CheckIfUserExists(model.Email))
            {
                throw new Exception("User with this email already exists");
            }
            return await _userService.CreateUser(model);
        }
    }
}
