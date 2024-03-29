﻿using API.Models.Attach;
using DAL;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;

namespace API.Services
{
    public class AttachService
    {
        private readonly DataContext _context;

        public AttachService(DataContext context)
        {
            _context = context;
        }

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
                Size = file.Length,
            };

            var newPath = Path.Combine(tempPath, metadata.Id.ToString());

            var fileInfo = new FileInfo(newPath);
            if (fileInfo.Exists)
            {
                throw new Exceptions.FileExistsException();
            }
            else
            {
                if (fileInfo.Directory == null)
                    throw new DirectoryNotFoundException("Temp directory doesn't exist");

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

        public void PurgeAttachFromPermanentStorage(string path)
        {
            var file = new FileInfo(path);
            if (!file.Exists)
            {
                throw new FileNotFoundException("Requested attachment file doesn't exist in temp folder");
            }

            System.IO.File.Delete(path);
        }
    
        public string UploadAttachToPermanentStorage(MetadataModel attachmentMetadata)
        {
            string pathToAttachment = Path.Combine(Path.GetTempPath(), attachmentMetadata.Id.ToString());

            //first, check if file with such metadata is already in the DB
            var attach = _context.Attaches.FirstOrDefault(a => a.Id == attachmentMetadata.Id);
            if(attach != null)
            {
                return attach.FilePath;
            }

            var file = new FileInfo(pathToAttachment);
            if (!file.Exists)
            {
                throw new FileNotFoundException("Requested attachment file doesn't exist in temp folder");
            }

            var permanentAttachPath = Path.Combine(Directory.GetCurrentDirectory(), "attaches", attachmentMetadata.Id.ToString());
            var permanentAttachFileInfo = new FileInfo(permanentAttachPath);

            if (permanentAttachFileInfo.Exists)
                return permanentAttachPath;

            if (permanentAttachFileInfo.Directory != null && !permanentAttachFileInfo.Directory.Exists)
            {
                permanentAttachFileInfo.Directory?.Create();
            }

            System.IO.File.Copy(file.FullName, permanentAttachPath);

            return permanentAttachPath;
        }
    }
}
