using API.Mapper.Actions;
using API.Models.Attach;
using API.Models.Post;
using API.Models.Post.Comment;
using API.Models.Post.Reaction;
using API.Models.User;
using AutoMapper;
using Common;

namespace API.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<CreateUserModel, DAL.Entities.User>()
                .ForMember(u => u.Id, m => m.MapFrom(s => Guid.NewGuid()))
                .ForMember(u => u.PasswordHashed, m => m.MapFrom(s => HashHelper.GetHash(s.Password)))
                .ForMember(u => u.CreateDate, m => m.MapFrom(s => DateTime.UtcNow));

            CreateMap<DAL.Entities.User, GetUserModel>()
                .ForMember(u => u.SubscribersCount, m => m.MapFrom(u => u.Subscribers!.Count()))
                .ForMember(u => u.SubscriptionsCount, m => m.MapFrom(u => u.Subscriptions!.Count()));
            CreateMap<DAL.Entities.User, GetUserModelWithAvatar>()
                .ForMember(u => u.SubscribersCount, m => m.MapFrom(u => u.Subscribers!.Count()))
                .ForMember(u => u.SubscriptionsCount, m => m.MapFrom(u => u.Subscriptions!.Count()))
                .AfterMap<UserToUserModelAvatarAction>();

            CreateMap<DAL.Entities.Avatar, AttachModel>();
            CreateMap<DAL.Entities.PostPhoto, AttachModel>();

            CreateMap<MetadataModel, DAL.Entities.Avatar>();
            CreateMap<MetadataModel, DAL.Entities.PostPhoto>();

            CreateMap<DAL.Entities.Comment, GetCommentModel>()
                .ForMember(u => u.ReactionsCount, m => m.MapFrom(s => s.CommentReactions!.Count));
            CreateMap<DAL.Entities.Comment, GetCommentContentModel>();
            CreateMap<CreateCommentModel, DAL.Entities.Comment>()
                .ForMember(u => u.Id, m => m.MapFrom(s => Guid.NewGuid()))
                .ForMember(u => u.CreationDate, m => m.MapFrom(s => DateTime.UtcNow));

            CreateMap<DAL.Entities.Post, GetPostModel>()
                .ForMember(u => u.CommentsCount, m=>m.MapFrom(s => s.PostComments!.Count))
                .ForMember(u => u.ReactionsCount, m=>m.MapFrom(s => s.PostReactions!.Count));
            CreateMap<DAL.Entities.Post, GetPostContentModel>();
            CreateMap<CreatePostModel, DAL.Entities.Post>()
                .ForMember(u => u.Id, m => m.MapFrom(s => Guid.NewGuid()))
                .ForMember(u => u.CreationDate, m => m.MapFrom(s => DateTime.UtcNow));

            CreateMap<DAL.Entities.Reaction, GetReactionModel>();
            CreateMap<CreateReactionModel, DAL.Entities.PostReaction>()
                .ForMember(u => u.CreationDate, m => m.MapFrom(s => DateTime.UtcNow));
            CreateMap<CreateReactionModel, DAL.Entities.CommentReaction>()
                .ForMember(u => u.CreationDate, m => m.MapFrom(s => DateTime.UtcNow)); 

            CreateMap<DAL.Entities.PostPhoto, GetPostPhotoModel>().AfterMap<PostPhotoToPostPhotoModelAction>();
        }
    }
}
