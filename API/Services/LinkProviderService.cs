using API.Models.Attach;
using API.Models.User;
using DAL.Entities;

namespace API.Services
{
    public class LinkProviderService
    {
        public Func<Guid, string?>? PostContentLinkGenerator;
        public Func<Guid, string?>? UserAvatarLinkGenerator;

        public void InjectAvatarURL(User s, GetUserModelWithAvatar d)
        {
            d.LinkToAvatar = s.Avatar == null ? null : UserAvatarLinkGenerator?.Invoke(s.Id);
        }

        public void InjectPostPhotoURL(PostPhoto photo, GetPostPhotoModel photoModel)
        {
            photoModel.URL = PostContentLinkGenerator?.Invoke(photo.Id);
        }
    }
}
