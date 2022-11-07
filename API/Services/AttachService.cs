using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Services
{
    public class AttachService
    {
        public async Task<List<MetadataModel>> UploadMultipleFiles(ICollection<IFormFile> files)
        {
            var metaList = new List<MetadataModel>();
            foreach (var file in files)
            {
                metaList.Add(await UploadFile(file));
            }
            return metaList;
        }
        private async Task<MetadataModel> UploadFile(IFormFile file)
        {
            var tempPath = Path.GetTempPath();
            var metadata = new MetadataModel
            {
                Id = Guid.NewGuid(),
                Name = file.FileName,
                MimeType = file.ContentType,
                FileSize = file.Length,
            };

            var newPath = Path.Combine(tempPath, metadata.Id.ToString());

            var fileInfo = new FileInfo(newPath);
            if (fileInfo.Exists)
            {
                throw new Exception("Such file already exists (for some reason)");
            }
            else
            {
                if (fileInfo.Directory == null)
                    throw new Exception("Temp doesn't exist");

                if (!fileInfo.Directory.Exists)
                {
                    fileInfo.Directory?.Create();
                }
            }

            using (var stream = System.IO.File.Create(newPath))
            {
                await file.CopyToAsync(stream);
            }

            return metadata;
        }
    

    }
}
