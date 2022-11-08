using API.Models.Attach;
using API.Models.Post;
using API.Models.Post.Comment;
using API.Models.User;
using AutoMapper;
using Common;

namespace API
{
    public class MapperProfile: Profile
    {
        public MapperProfile()
        {
            CreateMap<CreateUserModel, DAL.Entities.User>()
                .ForMember(u => u.Id, m => m.MapFrom(s => Guid.NewGuid()))
                .ForMember(u => u.PasswordHashed, m=> m.MapFrom(s => HashHelper.GetHash(s.Password)))
                .ForMember(u => u.CreateDate, m => m.MapFrom(s => DateTime.UtcNow));

            CreateMap<DAL.Entities.User, GetUserModel>();

            CreateMap<DAL.Entities.Avatar, AttachModel>();
            CreateMap<DAL.Entities.PostPhoto, AttachModel>();

            CreateMap<DAL.Entities.Comment, GetCommentModel>()
                .ForMember(u => u.Author, m=> m.Ignore());
            CreateMap<CreateCommentModel, DAL.Entities.Comment>()
                .ForMember(u => u.Id, m => m.MapFrom(s => Guid.NewGuid()))
                .ForMember(u => u.CreationDate, m => m.MapFrom(s => DateTime.UtcNow));

            CreateMap<DAL.Entities.Post, GetPostModel>()
                .ForMember(u => u.Author, m=>m.Ignore());
            CreateMap<CreatePostModel, DAL.Entities.Post>()
                .ForMember(u => u.Id, m => m.MapFrom(s => Guid.NewGuid()))
                .ForMember(u => u.CreationDate, m => m.MapFrom(s => DateTime.UtcNow))
                .ForMember(u => u.PostAttachments, m => m.Ignore());


            CreateMap<DAL.Entities.PostPhoto, GetPostPhotoModel>();
        }
    }
}
