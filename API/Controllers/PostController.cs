using API.Models.Post;
using API.Models.Post.Comment;
using API.Services;
using Common.Consts;
using Common.Extensions;
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
    public class PostController : Controller
    {
        private readonly PostService _postService;
        public PostController(PostService postService)
        {
            _postService = postService;
            if (postService != null)
            {
                postService.SetContentLinkGenerator(x =>
                Url.Action(nameof(GetPostPhotoByID), new { photoID = x }));
                postService.SetAvatarLinkGenerator(x =>
                Url.Action(nameof(UserController.GetUserAvatar), "User", new { userId = x.Id, download = false }));
            }
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

        [HttpGet]
        [AllowAnonymous]
        public async Task<FileResult> GetPostPhotoByID(Guid photoID, bool download = false)
        {
            var attach = await _postService.GetPostAttachByID(photoID);
            var fs = new FileStream(attach.FilePath, FileMode.Open);
            if (download)
                return File(fs, attach.MimeType, attach.Name);
            else
                return File(fs, attach.MimeType);
        }
    }
}
