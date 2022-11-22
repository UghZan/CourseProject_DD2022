using API.Models.Post;
using API.Models.Post.Comment;
using API.Models.Post.Reaction;
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

        #region Posts
        [HttpPost]
        public async Task<Guid> CreatePost(CreatePostModel model)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.userId);
            if (userId.Equals(default))
            {
                throw new UnauthorizedAccessException();
            }
            return await _postService.CreatePost(userId, model);

        }

        [HttpGet]
        public async Task<GetPostModel> GetPost(Guid postID)
        {
            return await _postService.GetPostByID(postID);
        }

        [HttpDelete]
        public async Task RemovePost(Guid postID)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.userId);
            if (userId.Equals(default))
            {
                throw new UnauthorizedAccessException();
            }
            await _postService.RemovePost(postID, userId);
        }

        [HttpGet]
        public async Task<IEnumerable<GetPostModel>> GetUserPosts(Guid userId, int amount = 5, int startingFrom = 0)
        {
            return await _postService.GetPostsByUser(userId, amount, startingFrom);
        }
        #endregion
        #region Comments
        [HttpPost]
        public async Task<Guid> CreateCommentOnPost(Guid postID, CreateCommentModel commentModel)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.userId);
            if (userId.Equals(default))
            {
                throw new UnauthorizedAccessException();
            }
            return await _postService.CreateCommentForPost(userId, postID, commentModel);
        }

        [HttpDelete]
        public async Task RemoveComment(Guid commentId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.userId);
            if (userId.Equals(default))
            {
                throw new UnauthorizedAccessException();
            }
            await _postService.RemoveComment(commentId, userId);
        }

        [HttpGet]
        public async Task<IEnumerable<GetCommentModel>> GetPostComments(Guid postID)
        {
            return await _postService.GetCommentsForPost(postID);
        }
        #endregion
        #region Reactions
        [HttpPost]
        public async Task CreateReactionOnPost(Guid postID, CreateReactionModel reactModel)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.userId);
            if (userId.Equals(default))
            {
                throw new UnauthorizedAccessException();
            }
            await _postService.CreateReactionForPost(userId, postID, reactModel);
        }

        [HttpPost]
        public async Task CreateReactionOnComment(Guid commentID, CreateReactionModel reactModel)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.userId);
            if (userId.Equals(default))
            {
                throw new UnauthorizedAccessException();
            }
            await _postService.CreateReactionForComment(userId, commentID, reactModel);
        }

        [HttpDelete]
        public async Task RemoveReactionFromPost(Guid postID)
        {
            //considering that it's 1 reaction per user-post pair, it's enough to use only postID as an argument
            var userId = User.GetClaimValue<Guid>(ClaimNames.userId);
            if (userId.Equals(default))
            {
                throw new UnauthorizedAccessException();
            }
            await _postService.RemoveReactionFromPost(postID, userId);
        }

        [HttpDelete]
        public async Task RemoveReactionFromComment(Guid commentID)
        {
            //considering that it's 1 reaction per user-post pair, it's enough to use only postID as an argument
            var userId = User.GetClaimValue<Guid>(ClaimNames.userId);
            if (userId.Equals(default))
            {
                throw new UnauthorizedAccessException();
            }
            await _postService.RemoveReactionFromComment(commentID, userId);
        }

        [HttpGet]
        public async Task<IEnumerable<GetReactionModel>> GetPostReactions(Guid postID)
        {
            return await _postService.GetReactionsForPost(postID);
        }

        [HttpGet]
        public async Task<IEnumerable<GetReactionModel>> GetCommentReactions(Guid postID)
        {
            return await _postService.GetReactionsForComment(postID);
        }
        #endregion
    }
}
