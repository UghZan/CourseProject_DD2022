using API.Models;
using API.Services;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

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

        [HttpPost]
        [Authorize]
        public async Task AddAvatarForUser(MetadataModel avatarMetadata)
        {
            var user = await GetCurrentUser();

            await _userService.AddAvatarForUser(user.Id, avatarMetadata);
        }

        [HttpGet]
        public async Task<FileResult> GetUserAvatar(Guid userId)
        {
            var attach = await _userService.GetUserAvatar(userId);

            return File(System.IO.File.ReadAllBytes(attach.FilePath), attach.MimeType);
        }

        [HttpGet]
        public async Task<FileResult> DownloadAvatar(Guid userId)
        {
            var attach = await _userService.GetUserAvatar(userId);

            HttpContext.Response.ContentType = attach.MimeType;
            FileContentResult result = new FileContentResult(System.IO.File.ReadAllBytes(attach.FilePath), attach.MimeType)
            {
                FileDownloadName = attach.Name
            };

            return result;
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
            else
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
