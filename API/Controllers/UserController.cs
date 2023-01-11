using API.Exceptions;
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

        #region Basic Users Functionality
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
        #endregion
        #region Subs
        [HttpPost]
        public async Task SubscribeToUser(Guid targetId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.userId);
            if (userId.Equals(default))
            {
                throw new UnauthorizedAccessException();
            }
            if(userId.Equals(targetId))
            {
                throw new Exceptions.InvalidOperationException("subscription to self");
            }
            await _userService.SubscribeToUser(userId, targetId);
        }

        [HttpPost]
        public async Task UnsubscribeFromUser(Guid targetId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.userId);
            if (userId.Equals(default))
            {
                throw new UnauthorizedAccessException();
            }
            if (userId.Equals(targetId))
            {
                throw new Exceptions.InvalidOperationException("unsubscription from self");
            }
            await _userService.UnsubscribeFrom(userId, targetId);
        }

        [HttpGet]
        public async Task<ICollection<GetUserModelWithAvatar>?> GetUserSubscriptions(Guid userId)
        {
            return await _userService.GetUserSubscriptions(userId);
        }

        [HttpGet]
        public async Task<ICollection<GetUserModelWithAvatar>?> GetUserSubscribers(Guid userId)
        {
            return await _userService.GetUserSubscribers(userId);
        }

        [HttpGet]
        public async Task<bool> IsUserSubscribedToTarget(Guid targetId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.userId);
            if (userId.Equals(default))
            {
                throw new UnauthorizedAccessException();
            }
            return await _userService.IsUserSubscribedToTarget(userId, targetId);
        }
        #endregion
    }
}
