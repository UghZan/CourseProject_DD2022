using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AttachController : Controller
    {
        private readonly AttachService _attachService;

        public AttachController(AttachService attachService)
        {
            _attachService = attachService;
        }

        [HttpPost]
        public async Task<List<MetadataModel>> UploadFiles([FromForm] List<IFormFile> files)
        {
            return await _attachService.UploadMultipleFiles(files);
        }

        [HttpPost]
        private async Task<MetadataModel> UploadFile([FromForm] IFormFile file)
        {
            return await _attachService.UploadFile(file);
        }
    }
}
