using API.Models.Attach;
using API.Models.User;
using API.Services;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common.Consts;
using Common.Extensions;
using Common.Extentions;
using DAL;
using DAL.Entities;
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
    [ApiExplorerSettings(GroupName = "API")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService, LinkProviderService linkProviderService)
        {
            _userService = userService;

            linkProviderService.UserAvatarLinkGenerator = x => Url.ControllerAction<AttachController>(nameof(AttachController.GetUserAvatar), new
            {
                userId = x,
            });
        }

        [HttpPost]
        public async Task AddAvatarForUser(MetadataModel avatarMetadata)
        {
            var user = await GetCurrentUser();

            await _userService.AddAvatarForUser(user.Id, avatarMetadata);
        }

        [HttpGet]
        public async Task<GetUserModelWithAvatar> GetCurrentUser()
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.userId);
            if (userId.Equals(default))
            {
                throw new UnauthorizedAccessException();
            }
            return await _userService.GetUserModelWithAvatarByID(userId);
        }

        [HttpGet]
        public async Task<IEnumerable<GetUserModelWithAvatar>> GetUsers()
        {
            return await _userService.GetUsers();
        }
    }
}
