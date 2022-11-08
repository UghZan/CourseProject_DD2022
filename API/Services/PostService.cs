using API.Models;
using AutoMapper;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Linq;

namespace API.Services
{
    public class PostService
    {
        private readonly IMapper _mapper;
        private readonly UserService _userService;
        private readonly AttachService _attachService;
        private readonly DataContext _context;
        private Func<Guid, string?>? _postContentLinkGenerator;
        private Func<UserModel, string?>? _userAvatarLinkGenerator;

        public void SetContentLinkGenerator(Func<Guid, string?>? linkGenerator)
        {
            _postContentLinkGenerator = linkGenerator;
        }
        public void SetAvatarLinkGenerator(Func<UserModel, string?>? linkGenerator)
        {
            _userAvatarLinkGenerator = linkGenerator;
        }

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
            var newPost = _mapper.Map<Post>(createPostModel);
            newPost.Author = user;

            if (!createPostModel.PostAttachments.IsNullOrEmpty())
            {
                newPost.PostAttachments = new List<PostPhoto>();
                foreach (MetadataModel attachment in createPostModel.PostAttachments)
                {
                    var pathToAttachment = _attachService.UploadAttachToPermanentStorage(attachment);
                    newPost.PostAttachments.Add(new PostPhoto
                    {
                        Author = user,
                        MimeType = attachment.MimeType,
                        FilePath = pathToAttachment,
                        Name = attachment.Name,
                        Size = attachment.FileSize,
                        Post = newPost
                    }
                    );
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

            var newComment = _mapper.Map<Comment>(commentModel);
            newComment.Author = user;
            newComment.ParentPost = post;

            await _context.Comments.AddAsync(newComment);
            await _context.SaveChangesAsync();
            return newComment.Id;
        }
        public async Task<IEnumerable<GetCommentModel>> GetCommentsForPost(Guid postID)
        {
            var post = await _context.Posts.Include(x => x.PostComments).ThenInclude(c => c.Author).FirstOrDefaultAsync(p => p.Id == postID);
            if (post == null)
            {
                throw new Exception("No such post found");
            }

            List<GetCommentModel> comments = new();
            foreach(Comment c in post.PostComments)
            {
                var author = new UserModelWithAvatar(await _userService.GetUserModelByID(c.Author.Id), _userAvatarLinkGenerator);
                var commenModel = _mapper.Map<GetCommentModel>(c);
                commenModel.Author = author;
                comments.Add(commenModel);
            }
            return comments;
        }

        public async Task<GetPostModel> GetPostByID(Guid postID)
        {
            var post = await _context.Posts.Include(p => p.PostAttachments).Include(p => p.Author).FirstOrDefaultAsync(p => p.Id == postID);
            if (post == null)
            {
                throw new Exception("Post not found");
            }
            return await PostToPostModel(post);
        }

        private async Task<GetPostModel> PostToPostModel(Post p)
        {
            var author = new UserModelWithAvatar(await _userService.GetUserModelByID(p.Author.Id), _userAvatarLinkGenerator);

            var postModel = new GetPostModel
            {
                PostContent = p.PostContent,
                Author = author,
                CreationDate = p.CreationDate,
                PostAttachments = p.PostAttachments.Select(
                    p => new GetPostPhotoModel(p, _postContentLinkGenerator)).ToArray()
            };
            return postModel;
        }

        public async Task<IEnumerable<GetPostModel>> GetPostsByUser(Guid userID, int amount, int startingFrom)
        {
            var posts = await _context.Posts
                .Include(x => x.Author).ThenInclude(x => x.Avatar)
                .Include(x => x.PostAttachments).AsNoTracking().Where(x => x.AuthorId == userID).Take(amount).Skip(startingFrom).ToListAsync();
            List<GetPostModel> userPosts = new List<GetPostModel>();
            foreach(Post p in posts)
            {
                userPosts.Add(await PostToPostModel(p));
            }
            return userPosts;
            
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
