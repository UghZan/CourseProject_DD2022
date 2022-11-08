using API.Models;
using AutoMapper;
using Common;

namespace API
{
    public class MapperProfile: Profile
    {
        public MapperProfile()
        {
            CreateMap<Models.CreateUserModel, DAL.Entities.User>()
                .ForMember(u => u.Id, m => m.MapFrom(s => Guid.NewGuid()))
                .ForMember(u => u.PasswordHashed, m=> m.MapFrom(s => HashHelper.GetHash(s.Password)))
                .ForMember(u => u.CreateDate, m => m.MapFrom(s => DateTime.UtcNow));

            CreateMap<DAL.Entities.User, Models.UserModel>();

            CreateMap<DAL.Entities.Avatar, Models.AttachModel>();
            CreateMap<DAL.Entities.PostPhoto, Models.AttachModel>();

            CreateMap<DAL.Entities.Comment, Models.GetCommentModel>()
                .ForMember(u => u.Author, m=> m.Ignore());
            CreateMap<Models.CreateCommentModel, DAL.Entities.Comment>()
                .ForMember(u => u.Id, m => m.MapFrom(s => Guid.NewGuid()))
                .ForMember(u => u.CreationDate, m => m.MapFrom(s => DateTime.UtcNow));

            CreateMap<DAL.Entities.Post, Models.GetPostModel>()
                .ForMember(u => u.Author, m=>m.Ignore());
            CreateMap<Models.CreatePostModel, DAL.Entities.Post>()
                .ForMember(u => u.Id, m => m.MapFrom(s => Guid.NewGuid()))
                .ForMember(u => u.CreationDate, m => m.MapFrom(s => DateTime.UtcNow))
                .ForMember(u => u.PostAttachments, m => m.Ignore());


            CreateMap<DAL.Entities.PostPhoto, Models.GetPostPhotoModel>();
        }
    }
}
