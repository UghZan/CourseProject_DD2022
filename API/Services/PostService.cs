using API.Models;
using AutoMapper;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public class PostService
    {
        private readonly IMapper _mapper;
        private readonly UserService _userService;
        private readonly AttachService _attachService;
        private readonly DataContext _context;

        public PostService(IMapper mapper, DataContext context, UserService userService, AttachService attachService)
        {
            _mapper = mapper;
            _context = context;
            _userService = userService;
            _attachService = attachService;
        }

        public async Task<Guid> CreatePost(Guid userID, CreatePostModel createPostModel)
        {
            var user = await _userService.GetUserByID(userID);
            var newPost = new Post()
            {
                Id = Guid.NewGuid(),
                Author = user,
                PostContent = createPostModel.PostContent,
                CreationDate = DateTime.UtcNow,
                PostAttachments = new List<PostPhoto>(),
                PostComments = new List<Comment>()
            };
            if (!createPostModel.PostAttachments.IsNullOrEmpty())
            {
                foreach (MetadataModel attachment in createPostModel.PostAttachments)
                {
                    var pathToAttachment = _attachService.UploadAttachToPermanentStorage(attachment);
                    var attach = new PostPhoto
                    {
                        Author = user,
                        MimeType = attachment.MimeType,
                        FilePath = pathToAttachment,
                        Name = attachment.Name,
                        Size = attachment.FileSize,
                        Post = newPost
                    };
                    newPost.PostAttachments.Add(attach);
                }
            }
            await _context.Posts.AddAsync(newPost);
            await _context.SaveChangesAsync();
            return newPost.Id;
        }

        public async Task<Guid> CreateCommentForPost(Guid userID, Guid postID, CreateCommentModel commentModel)
        {
            var user = await _userService.GetUserByID(userID);
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postID);
            if(post == null)
            {
                throw new Exception("No such post found");
            }
            var newComment = new Comment
            {
                Id = Guid.NewGuid(),
                Author = user,
                PostContent = commentModel.PostContent,
                CreationDate = DateTime.UtcNow,
                ParentPost = post
            };
            await _context.Comments.AddAsync(newComment);
            await _context.SaveChangesAsync();
            return newComment.Id;
        }
        public async Task<List<GetCommentModel>> GetCommentsForPost(Guid postID)
        {
            var post = await _context.Posts.Include(x => x.PostComments).FirstOrDefaultAsync(p => p.Id == postID);
            if (post == null)
            {
                throw new Exception("No such post found");
            }

            List<GetCommentModel> comments = new List<GetCommentModel>();

            foreach(Comment c in post.PostComments)
            {
                comments.Add(await GetCommentModelById(postID, c.Id));
            }
            return comments;
        }
        private async Task<GetCommentModel> GetCommentModelById(Guid parentPostID, Guid commentID)
        {
            var originalComment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentID);
            if (originalComment == null)
                throw new Exception("No comment by that ID found");

            GetCommentModel commentModel = new GetCommentModel
            {
                PostContent = originalComment.PostContent,
                AuthorId = originalComment.AuthorId,
                CreationDate = originalComment.CreationDate,
            };
            return commentModel;
                 
        }

        public async Task<GetPostModel> GetPostByID(Guid postID)
        {
            var post = await _context.Posts.Include(p => p.PostAttachments).FirstOrDefaultAsync(p => p.Id == postID);
            if (post == null)
            {
                throw new Exception("Post not found");
            }
            var postModel = new GetPostModel
            {
                PostContent = post.PostContent,
                AuthorId = post.AuthorId,
                CreationDate = post.CreationDate,
                PostAttachments = new List<PostPhotoModel>()
            };
            if (post.PostAttachments != null)
                foreach (PostPhoto attach in post.PostAttachments)
                {
                    postModel.PostAttachments.Add(new PostPhotoModel
                    {
                        MimeType = attach.MimeType,
                        AttachId = attach.Id
                    });
                }
            return postModel;
        }

        public async Task<AttachModel> GetPostAttachByID(Guid photoID)
        {
            var attach = await _context.PostPhotos.FirstOrDefaultAsync(p => p.Id == photoID);
            if (attach == null)
                throw new Exception("Couldn't find attachment");
            return _mapper.Map<AttachModel>(attach);
        }
    }
}
