using API.Models;
using API.Services;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task CreateUser(CreateUserModel model)
        {
            if(await _userService.CheckIfUserExists(model.Email))
            {
                throw new Exception("User with this email already exists");
            }
            await _userService.CreateUser(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<UserModel> GetCurrentUser()
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "userID").Value;
            if(Guid.TryParse(userIdString, out Guid userId))
            {
                return await _userService.GetUserModelByID(userId);
            }

            throw new Exception("You are not authorized");
        }

        [HttpGet]
        [Authorize]
        public async Task<List<UserModel>> GetUsers()
        {
            return await _userService.GetUsers();
        }
    }
}
