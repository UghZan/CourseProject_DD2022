using API.Models.Post;
using API.Models.Post.Comment;
using API.Services;
using Common.Consts;
using Common.Extensions;
using Common.Extentions;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
using System.Text;

namespace API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    [ApiExplorerSettings(GroupName = "API")]
    public class PostController : Controller
    {
        private readonly PostService _postService;

        public PostController(PostService postService, LinkProviderService linkProviderService)
        {
            _postService = postService;

            linkProviderService.UserAvatarLinkGenerator = x => Url.ControllerAction<AttachController>(nameof(AttachController.GetUserAvatar), new
            {
                userId = x,
            });
            linkProviderService.PostContentLinkGenerator = x => Url.ControllerAction<AttachController>(nameof(AttachController.GetPostPhotoByID), new
            {
                photoId = x,
            });
        }

        [HttpPost]
        public async Task<Guid> CreatePost(CreatePostModel model)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.userId);
            if (userId.Equals(default))
            {
                throw new Exception("You are not authorized");
            }
            return await _postService.CreatePost(userId, model);

        }

        [HttpGet]
        public async Task<GetPostModel> GetPost(Guid postID)
        {
            return await _postService.GetPostByID(postID);
        }

        [HttpGet]
        public async Task<IEnumerable<GetPostModel>> GetUserPosts(Guid userId, int amount = 5, int startingFrom = 0)
        {
            return await _postService.GetPostsByUser(userId, amount, startingFrom);
        }

        [HttpPost]
        public async Task<Guid> CreateCommentOnPost(Guid postID, CreateCommentModel commentModel)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.userId);
            if (userId.Equals(default))
            {
                throw new Exception("You are not authorized");
            }
            return await _postService.CreateCommentForPost(userId, postID, commentModel);
        }

        [HttpGet]
        public async Task<IEnumerable<GetCommentModel>> GetPostComments(Guid postID)
        {
            return await _postService.GetCommentsForPost(postID);
        }
    }
}
