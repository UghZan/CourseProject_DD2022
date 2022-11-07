﻿using DAL.Entities;

namespace API.Models
{
    public class GetPostModel
    {
        public string? PostContent { get; set; }
        public ICollection<string> PostAttachments { get; set; } = null!;
        public Guid AuthorId { get; set; }
        public DateTimeOffset CreationDate { get; set; }
    }
}
