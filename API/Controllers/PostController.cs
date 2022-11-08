using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
using System.Text;

namespace API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PostController : Controller
    {
        private readonly PostService _postService;
        private readonly UserService _userService;
        public PostController(PostService postService, UserService userService)
        {
            _postService = postService;
            _userService = userService;
        }

        [HttpPost]
        [Authorize]
        public async Task<Guid> CreatePost(CreatePostModel model)
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "userID").Value;
            if (Guid.TryParse(userIdString, out Guid userId))
            {
                return await _postService.CreatePost(userId, model);
            }
            else
                throw new Exception("You are not authorized");
        }

        [HttpGet]
        public async Task<GetPostModel> GetPost(Guid postID)
        {
            var postModel = await _postService.GetPostByID(postID);
            if (postModel.PostAttachments != null)
            {
                //get URL of method that shows photos and build a link for current photo attachment
                foreach (PostPhotoModel attachId in postModel.PostAttachments)
                {
                    attachId.URL = Url.Action("GetPostPhotoByID", new {photoID = attachId.AttachId});
                }
            }
            return postModel;
        }

        [HttpPost]
        [Authorize]
        public async Task<Guid> CreateCommentOnPost(Guid postID, CreateCommentModel commentModel)
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "userID").Value;
            if (Guid.TryParse(userIdString, out Guid userId))
            {
                return await _postService.CreateCommentForPost(userId, postID, commentModel);
            }
            else
                throw new Exception("You are not authorized");
        }

        [HttpGet]
        public async Task<List<GetCommentModel>> GetPostComments(Guid postID)
        {
            return await _postService.GetCommentsForPost(postID);
        }

        [HttpGet]
        public async Task<FileResult> GetPostPhotoByID(Guid photoID)
        {
            var attach = await _postService.GetPostAttachByID(photoID);

            return File(System.IO.File.ReadAllBytes(attach.FilePath), attach.MimeType);
        }
    }
}
