using API.Models.Attach;
using API.Models.User;
using API.Services;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common.Consts;
using Common.Extensions;
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
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
            if (userService != null)
                _userService.SetLinkGenerator(x =>
                Url.Action(nameof(GetUserAvatar), new { userId = x.Id, download = false }));
        }

        [HttpPost]
        public async Task AddAvatarForUser(MetadataModel avatarMetadata)
        {
            var user = await GetCurrentUser();

            await _userService.AddAvatarForUser(user.Id, avatarMetadata);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<FileStreamResult> GetUserAvatar(Guid userId, bool download = false)
        {
            var attach = await _userService.GetUserAvatar(userId);
            var fs = new FileStream(attach.FilePath, FileMode.Open);
            if (download)
                return File(fs, attach.MimeType, attach.Name);
            else
                return File(fs, attach.MimeType);

        }

        [HttpGet]
        public async Task<GetUserModel> GetCurrentUser()
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.userId);
            if (userId.Equals(default))
            {
                throw new Exception("You are not authorized");
            }
            return await _userService.GetUserModelByID(userId);
        }

        [HttpGet]
        public async Task<IEnumerable<GetUserModelWithAvatar>> GetUsers()
        {
            return await _userService.GetUsers();
        }
    }
}
