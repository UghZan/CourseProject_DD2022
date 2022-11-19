using API.Models.User;
using API.Services;
using AutoMapper;
using DAL.Entities;

namespace API.Mapper.Actions
{
    public class UserToUserModelAvatarAction : IMappingAction<User, GetUserModelWithAvatar>
    {
        private LinkProviderService _linkService;
        public UserToUserModelAvatarAction(LinkProviderService linkService)
        {
            _linkService = linkService;
        }
        public void Process(User source, GetUserModelWithAvatar destination, ResolutionContext context) =>
            _linkService.InjectAvatarURL(source, destination);

    }
}
