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
using System.ComponentModel.Design;
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
        public async Task<Guid> RecreatePost(Guid postID, Guid userID, CreatePostModel newPostModel)
        {
            var originalPost = await _context.Posts.Include(p => p.PostAttachments).Include(p => p.Author).FirstOrDefaultAsync(p => p.Id == postID);
            if (originalPost == null)
            {
                throw new PostNotFoundException();
            }
            if (userID != originalPost.AuthorId)
            {
                throw new PermissionException("editing post");
            }
            originalPost.PostContent = newPostModel.PostContent;

            if (!originalPost.PostAttachments.IsNullOrEmpty())
            {
                foreach(PostPhoto p in originalPost.PostAttachments)
                {
                    _attachService.PurgeAttachFromPermanentStorage(p.FilePath);
                    _context.PostPhotos.Remove(p);
                }
                originalPost.PostAttachments.Clear();
            }
            if (!newPostModel.PostAttachments.IsNullOrEmpty())
            {
                foreach (MetadataModel attachment in newPostModel.PostAttachments)
                {
                    var pathToAttachment = _attachService.UploadAttachToPermanentStorage(attachment);
                    var postPhoto = _mapper.Map<PostPhoto>(attachment);
                    postPhoto.FilePath = pathToAttachment;
                    postPhoto.Post = originalPost;
                    postPhoto.Author = originalPost.Author;
                    await _context.PostPhotos.AddAsync(postPhoto);
                }
            }
            _context.Posts.Update(originalPost);
            await _context.SaveChangesAsync();
            return originalPost.Id;
        }
        public async Task<GetPostModel> GetPostByID(Guid postID)
        {
            var post = await _context.Posts.Include(p => p.PostComments).Include(p=>p.PostReactions).Include(p => p.PostAttachments).Include(p => p.Author).ThenInclude(u => u.Avatar).FirstOrDefaultAsync(p => p.Id == postID);
            if (post == null)
            {
                throw new PostNotFoundException();
            }
            return _mapper.Map<GetPostModel>(post);
        }
        public async Task<IEnumerable<GetPostModel>> GetPostsByUser(Guid userID, int amount, int startingFrom)
        {
            var posts = await _context.Posts
                .Include(x => x.Author).ThenInclude(x => x.Avatar)
                .Include(x => x.PostAttachments).Include(p => p.PostComments).Include(p => p.PostReactions).AsNoTracking().OrderByDescending(x => x.CreationDate).Where(x => x.AuthorId == userID)
                .Take(amount).Skip(startingFrom).Select(x => _mapper.Map<GetPostModel>(x)).ToListAsync();
            return posts;
            
        }
        public async Task<IEnumerable<GetPostModel>> GetPosts(int amount, int startingFrom)
        {
            var posts = await _context.Posts
                .Include(x => x.Author).ThenInclude(x => x.Avatar)
                .Include(x => x.PostAttachments).Include(p => p.PostComments).Include(p => p.PostReactions).AsNoTracking().OrderByDescending(x => x.CreationDate)
                .Take(amount).Skip(startingFrom).Select(x => _mapper.Map<GetPostModel>(x)).ToListAsync();
            return posts;

        }
        public async Task<IEnumerable<GetPostModel>> GetPostsForUser(Guid userId, int amount, int startingFrom)
        {
            var user = await _userService.GetUserByID(userId);
            var applicableAuthors = new List<Guid>();
            if(user.Subscriptions != null) applicableAuthors = user.Subscriptions.Select(x => x.Id).ToList();
            applicableAuthors.Add(user.Id);
            var posts = await _context.Posts.Where(e => applicableAuthors.Contains(e.AuthorId))
                .Include(x => x.Author).ThenInclude(x => x.Avatar)
                .Include(x => x.PostAttachments).Include(p => p.PostComments).Include(p => p.PostReactions).AsNoTracking().OrderByDescending(x => x.CreationDate)
                .Take(amount).Skip(startingFrom).Select(x => _mapper.Map<GetPostModel>(x)).ToListAsync();
            return posts;

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
        public async Task<Guid> CreateCommentForPost(Guid postID, Guid userID, CreateCommentModel commentModel)
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
        public async Task<Guid> RecreateComment(Guid commentID, Guid userID, CreateCommentModel newCommentModel)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(p => p.Id == commentID);
            if (comment == null)
            {
                throw new CommentNotFoundException();
            }
            if (userID != comment.AuthorId)
            {
                throw new PermissionException("editing comment");
            }
            comment.PostContent = newCommentModel.PostContent;

            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
            return comment.Id;
        }
        public async Task<IEnumerable<GetCommentModel>> GetCommentsForPost(Guid postID)
        {
            var post = await _context.Posts.AsNoTracking().Include(x => x.PostComments).ThenInclude(c => c.CommentReactions).Include(x => x.PostComments).ThenInclude(c => c.Author).ThenInclude(a => a.Avatar).FirstOrDefaultAsync(p => p.Id == postID);
            if (post == null)
            {
                throw new PostNotFoundException();
            }

            return post.PostComments.Select(c => _mapper.Map<GetCommentModel>(c));
        }
        public async Task<GetCommentModel> GetCommentByID(Guid commentID)
        {
            var comment = await _context.Comments.Include(c => c.CommentReactions).Include(c => c.Author).ThenInclude(u => u.Avatar).FirstOrDefaultAsync(p => p.Id == commentID);
            if (comment == null)
            {
                throw new CommentNotFoundException();
            }
            return _mapper.Map<GetCommentModel>(commentID);
        }
        public async Task RemoveComment(Guid commentId, Guid actorID)
        {
            var comment = await _context.Comments.Include(c => c.ParentPost).Include(p => p.Author).FirstOrDefaultAsync(p => p.Id == commentId);
            if (comment == null)
            {
                throw new CommentNotFoundException();
            }
            var user = await _userService.GetUserByID(actorID);
            //if user isn't an author of the comment, he shouldn't be able to delete it
            if (!CanRemoveComment(comment, user.Id))
            {
                throw new PermissionException("removing comment");
            }
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }
        public bool CanRemoveComment(Comment comment, Guid requesterId)
        {
            //only comment author OR comment's parent post author may remove comments
            return comment.ParentPost.AuthorId == requesterId || comment.AuthorId == requesterId;
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

        public async Task<int> GetUserReactionForPost(Guid userId, Guid postID)
        {
            var reaction = await _context.PostReactions.Include(x => x.ReactionAuthor).FirstOrDefaultAsync(p => p.ReactionAuthorId == userId && p.ReactionPostId == postID);
            if (reaction == null)
            {
                return -1;
            }
            return (int)reaction!.ReactionType;
        }
        public async Task<int> GetUserReactionForComment(Guid userId, Guid commentID)
        {
            var reaction = await _context.CommentReactions.Include(x => x.ReactionAuthor).FirstOrDefaultAsync(p => p.ReactionAuthorId == userId && p.ReactionCommentId == commentID);
            if(reaction == null)
            {
                return -1;
            }
            return (int)reaction!.ReactionType;
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
