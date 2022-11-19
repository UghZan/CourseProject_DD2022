using API.Models.Attach;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "API")]
    public class AttachController : Controller
    {
        private readonly AttachService _attachService;
        private readonly PostService _postService;
        private readonly UserService _userService;

        public AttachController(AttachService attachService, PostService postService, UserService userService)
        {
            _attachService = attachService;
            _postService = postService;
            _userService = userService;
        }

        [HttpPost]
        public async Task<List<MetadataModel>> UploadFiles([FromForm] List<IFormFile> files)
        {
            return await _attachService.UploadMultipleFiles(files);
        }

        [HttpGet]
        [Route("{photoID}")]
        public async Task<FileStreamResult> GetPostPhotoByID(Guid photoID, bool download = false)
        {
            return GetAttach(await _postService.GetPostAttachByID(photoID), download);
        }

        [HttpGet]
        [Route("{userID}")]
        public async Task<FileStreamResult> GetUserAvatar(Guid userID, bool download = false)
        {
            return GetAttach(await _userService.GetUserAvatar(userID), download);

        }

        private FileStreamResult GetAttach(AttachModel attach, bool download)
        {
            var fs = new FileStream(attach.FilePath, FileMode.Open);
            var ext = Path.GetExtension(attach.Name);
            if (download)
                return File(fs, attach.MimeType, $"{attach.Id}{ext}");
            else
                return File(fs, attach.MimeType);

        }
    }
}
