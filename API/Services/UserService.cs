using API.Configs;
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
        private Func<GetUserModel, string?>? _userAvatarLinkGenerator;
        public void SetLinkGenerator(Func<GetUserModel, string?> linkGenerator)
        {
            _userAvatarLinkGenerator = linkGenerator;
        }
        public Func<GetUserModel, string?>? GetLinkGenerator() => _userAvatarLinkGenerator;

        public UserService(IMapper mapper, DataContext context, IOptions<AuthConfig> config, AttachService attachService)
        {
            _mapper = mapper;
            _context = context;
            _attachService = attachService;
        }

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
                throw new Exception("User is not found");

            var filePath = _attachService.UploadAttachToPermanentStorage(metadata);

            var avatar = new Avatar
            {
                Author = user,
                MimeType = metadata.MimeType,
                FilePath = filePath,
                Name = metadata.Name,
                Size = metadata.FileSize,
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
            var users = await _context.Users.AsNoTracking().ProjectTo<GetUserModel>(_mapper.ConfigurationProvider).ToListAsync();
            return users.Select(x => new GetUserModelWithAvatar(x, _userAvatarLinkGenerator));
        }

        public async Task<User> GetUserByID(Guid id)
        {
            var user = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                throw new Exception("No user by this id found");
            }
            return user;
        }

        public async Task<GetUserModel> GetUserModelByID(Guid id)
        {
            var user = await GetUserByID(id);

            return _mapper.Map<GetUserModel>(user);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
