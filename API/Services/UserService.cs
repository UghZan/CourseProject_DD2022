using API.Configs;
using API.Exceptions;
using API.Models.Attach;
using API.Models.User;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API.Services
{
    public class UserService : IDisposable
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly AttachService _attachService;
        private readonly LinkProviderService _linkService;

        public UserService(IMapper mapper, DataContext context, IOptions<AuthConfig> config, AttachService attachService, LinkProviderService linkProviderService)
        {
            _mapper = mapper;
            _context = context;
            _attachService = attachService;
            _linkService = linkProviderService;
        }

        #region Basic User Functionality
        public async Task<bool> CheckIfUserExists(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task DeleteUser(Guid id)
        {
            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (dbUser != null)
            {
                _context.Users.Remove(dbUser);
                await _context.SaveChangesAsync();
            }
            else
                throw new UserNotFoundException();
        }

        public async Task<Guid> CreateUser(CreateUserModel model)
        {
            var dbUser = _mapper.Map<User>(model);
            var temp = await _context.Users.AddAsync(dbUser);
            await _context.SaveChangesAsync();
            return temp.Entity.Id;
        }

        public async Task AddAvatarForUser(Guid userID, MetadataModel metadata)
        {
            var user = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(u => u.Id == userID);
            if (user == null)
                throw new UserNotFoundException();

            var filePath = _attachService.UploadAttachToPermanentStorage(metadata);

            var avatar = new Avatar
            {
                Author = user,
                MimeType = metadata.MimeType,
                FilePath = filePath,
                Name = metadata.Name,
                Size = metadata.Size,
                User = user
            };

            user.Avatar = avatar;
            await _context.SaveChangesAsync();
        }

        public async Task<AttachModel> GetUserAvatar(Guid userId)
        {
            var user = await GetUserByID(userId);
            var attach = _mapper.Map<AttachModel>(user.Avatar);
            return attach;
        }

        public async Task<IEnumerable<GetUserModelWithAvatar>> GetUsers()
        {
            var users = await _context.Users.AsNoTracking().ProjectTo<GetUserModelWithAvatar>(_mapper.ConfigurationProvider).ToListAsync();
            return users;
        }

        public async Task<User> GetUserByID(Guid id)
        {
            var user = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                throw new UserNotFoundException();
            }
            return user;
        }

        public async Task<GetUserModel> GetUserModelByID(Guid id)
        {
            var user = await GetUserByID(id);

            return _mapper.Map<GetUserModel>(user);
        }

        public async Task<GetUserModelWithAvatar> GetUserModelWithAvatarByID(Guid id)
        {
            var user = await GetUserByID(id);

            return _mapper.Map<GetUserModelWithAvatar>(user);
        }
        #endregion
        #region Subs
        public async Task SubscribeToUser(Guid requesterId, Guid targetId)
        {
            var user = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(u => u.Id == requesterId);
            if (user == null)
                throw new UserNotFoundException("request creator");
            var targetUser = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(u => u.Id == targetId);
            if (targetUser == null)
                throw new UserNotFoundException("request target");

            if (targetUser.Subscribers == null)
                targetUser.Subscribers = new List<User>();
            if (user.Subscriptions == null)
                user.Subscriptions = new List<User>();
            targetUser.Subscribers.Add(user);
            user.Subscriptions.Add(targetUser);

            await _context.SaveChangesAsync();
        }
        
        public async Task<ICollection<GetUserModelWithAvatar>?> GetUserSubscriptions(Guid userId)
        {
            var user = await _context.Users.Include(x => x.Subscriptions)?.ThenInclude(u => u.Avatar).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new UserNotFoundException();

            return user.Subscriptions?.Select(s => _mapper.Map<GetUserModelWithAvatar>(s)).ToList();
        }

        public async Task<ICollection<GetUserModelWithAvatar>?> GetUserSubscribers(Guid userId)
        {
            var user = await _context.Users.Include(x => x.Subscribers)?.ThenInclude(u => u.Avatar).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new UserNotFoundException();

            return user.Subscribers?.Select(s => _mapper.Map<GetUserModelWithAvatar>(s)).ToList();
        }

        #endregion
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
