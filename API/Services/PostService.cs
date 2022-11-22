using API.Exceptions;
using API.Models.Attach;
using API.Models.Post;
using API.Models.Post.Comment;
using API.Models.Post.Reaction;
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
        #region Posts
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
        public async Task<GetPostModel> GetPostByID(Guid postID)
        {
            var post = await _context.Posts.Include(p => p.PostAttachments).Include(p => p.Author).ThenInclude(u => u.Avatar).FirstOrDefaultAsync(p => p.Id == postID);
            var reactions =  _context.PostReactions.Where(r => r.ReactionPostId == postID).Count();
            if (post == null)
            {
                throw new PostNotFoundException();
            }
            var postModel = _mapper.Map<GetPostModel>(post);
            postModel.ReactionsCount = reactions;
            return postModel;
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
                var postModel = _mapper.Map<GetPostModel>(p);
                var reactions = _context.PostReactions.Where(r => r.ReactionPostId == p.Id).Count();
                postModel.ReactionsCount = reactions;
                userPosts.Add(_mapper.Map<GetPostModel>(p));
            }
            return userPosts;
            
        }
        public async Task<AttachModel> GetPostAttachByID(Guid photoID)
        {
            var attach = await _context.PostPhotos.FirstOrDefaultAsync(p => p.Id == photoID);
            if (attach == null)
                throw new AttachNotFoundException();
            return _mapper.Map<AttachModel>(attach);
        }
        public async Task RemovePost(Guid postID, Guid actorID)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postID);
            if (post == null)
            {
                throw new PostNotFoundException();
            }
            var user = await _userService.GetUserByID(actorID);
            //if user isn't an author of the post, he shouldn't be able to delete it
            if(post.AuthorId != user.Id) // could use some sort of admin permissions later
            {
                throw new PermissionException("removing post");
            }
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }
        #endregion
        #region Comments
        public async Task<Guid> CreateCommentForPost(Guid userID, Guid postID, CreateCommentModel commentModel)
        {
            var user = await _userService.GetUserByID(userID);
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postID);
            if (post == null)
            {
                throw new PostNotFoundException();
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
                throw new PostNotFoundException();
            }

            return post.PostComments.Select(c => _mapper.Map<GetCommentModel>(c));
        }
        public async Task RemoveComment(Guid commentId, Guid actorID)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(p => p.Id == commentId);
            if (comment == null)
            {
                throw new CommentNotFoundException();
            }
            var user = await _userService.GetUserByID(actorID);
            //if user isn't an author of the comment, he shouldn't be able to delete it
            if (comment.AuthorId != user.Id)
            {
                throw new PermissionException("removing comment");
            }
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }

        #endregion
        #region Reactions
        public async Task CreateReactionForPost(Guid userID, Guid postID, CreateReactionModel reactModel)
        {
            var user = await _userService.GetUserByID(userID);
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postID);

            if (post == null)
            {
                throw new PostNotFoundException();
            }

            if(reactModel.ReactionType > ReactionType.WOAH)
            {
                throw new InvalidReactionException(reactModel.ReactionType.ToString());
            }

            var newReaction = _mapper.Map<PostReaction>(reactModel);
            newReaction.ReactionPost = post;
            newReaction.ReactionAuthor = user;

            await _context.PostReactions.AddAsync(newReaction);
            await _context.SaveChangesAsync();
        }
        public async Task CreateReactionForComment(Guid userID, Guid commentID, CreateReactionModel reactModel)
        {
            var user = await _userService.GetUserByID(userID);
            var comment = await _context.Comments.FirstOrDefaultAsync(p => p.Id == commentID);

            if (comment == null)
            {
                throw new PostNotFoundException();
            }

            if (reactModel.ReactionType > ReactionType.WOAH)
            {
                throw new InvalidReactionException(reactModel.ReactionType.ToString());
            }

            var newReaction = _mapper.Map<CommentReaction>(reactModel);
            newReaction.ReactionComment = comment;
            newReaction.ReactionAuthor = user;

            await _context.CommentReactions.AddAsync(newReaction);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<GetReactionModel>> GetReactionsForPost(Guid postID)
        {
            var reactions = await _context.PostReactions.Include(x => x.ReactionAuthor).ThenInclude(a => a.Avatar).Where(p => p.ReactionPostId == postID).ToListAsync();

            return reactions.Select(c => _mapper.Map<GetReactionModel>(c));
        }
        public async Task<IEnumerable<GetReactionModel>> GetReactionsForComment(Guid commentID)
        {
            var reactions = await _context.CommentReactions.Include(x => x.ReactionAuthor).ThenInclude(a => a.Avatar).Where(p => p.ReactionCommentId == commentID).ToListAsync();

            return reactions.Select(c => _mapper.Map<GetReactionModel>(c));
        }
        public async Task RemoveReactionFromPost(Guid postID, Guid userID)
        {
            var reaction = await _context.PostReactions.FirstOrDefaultAsync(p => p.ReactionPostId == postID && p.ReactionAuthorId == userID);
            if (reaction == null)
            {
                throw new ReactionNotFoundException();
            }

            _context.PostReactions.Remove(reaction);
            await _context.SaveChangesAsync();
        }
        public async Task RemoveReactionFromComment(Guid commentID, Guid userID)
        {
            var reaction = await _context.CommentReactions.FirstOrDefaultAsync(p => p.ReactionCommentId == commentID && p.ReactionAuthorId == userID);
            if (reaction == null)
            {
                throw new ReactionNotFoundException();
            }

            _context.CommentReactions.Remove(reaction);
            await _context.SaveChangesAsync();
        }
        #endregion
    }
}
