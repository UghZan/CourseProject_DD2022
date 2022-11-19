using API.Models.Attach;
using API.Models.Post;
using API.Models.Post.Comment;
using API.Models.User;
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
        private readonly LinkProviderService _linkService;


        public PostService(IMapper mapper, DataContext context, UserService userService, AttachService attachService, LinkProviderService linkService)
        {
            _mapper = mapper;
            _context = context;
            _userService = userService;
            _attachService = attachService;
            _linkService = linkService;
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
                    var postPhoto = _mapper.Map<PostPhoto>(attachment);
                    postPhoto.FilePath = pathToAttachment;
                    postPhoto.Post = newPost;
                    postPhoto.Author = user;
                    newPost.PostAttachments.Add(postPhoto);
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
            var post = await _context.Posts.Include(x => x.PostComments).ThenInclude(c => c.Author).ThenInclude(a => a.Avatar).FirstOrDefaultAsync(p => p.Id == postID);
            if (post == null)
            {
                throw new Exception("No such post found");
            }

            return post.PostComments.Select(c => _mapper.Map<GetCommentModel>(c));
        }

        public async Task<GetPostModel> GetPostByID(Guid postID)
        {
            var post = await _context.Posts.Include(p => p.PostAttachments).Include(p => p.Author).ThenInclude(u => u.Avatar).FirstOrDefaultAsync(p => p.Id == postID);
            if (post == null)
            {
                throw new Exception("Post not found");
            }
            return _mapper.Map<GetPostModel>(post);
        }

        public async Task<IEnumerable<GetPostModel>> GetPostsByUser(Guid userID, int amount, int startingFrom)
        {
            var posts = await _context.Posts
                .Include(x => x.Author).ThenInclude(x => x.Avatar)
                .Include(x => x.PostAttachments).AsNoTracking().OrderByDescending(x => x.CreationDate).Where(x => x.AuthorId == userID)
                .Take(amount).Skip(startingFrom).ToListAsync();
            List<GetPostModel> userPosts = new List<GetPostModel>();
            foreach(Post p in posts)
            {
                userPosts.Add(_mapper.Map<GetPostModel>(p));
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
