﻿using API.Mapper.Actions;
using API.Models.Attach;
using API.Models.Post;
using API.Models.Post.Comment;
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

            CreateMap<DAL.Entities.User, GetUserModel>();
            CreateMap<DAL.Entities.User, GetUserModelWithAvatar>().AfterMap<UserToUserModelAvatarAction>();

            CreateMap<DAL.Entities.Avatar, AttachModel>();
            CreateMap<DAL.Entities.PostPhoto, AttachModel>();

            CreateMap<MetadataModel, DAL.Entities.Avatar>();
            CreateMap<MetadataModel, DAL.Entities.PostPhoto>();

            CreateMap<DAL.Entities.Comment, GetCommentModel>();
            CreateMap<CreateCommentModel, DAL.Entities.Comment>()
                .ForMember(u => u.Id, m => m.MapFrom(s => Guid.NewGuid()))
                .ForMember(u => u.CreationDate, m => m.MapFrom(s => DateTime.UtcNow));

            CreateMap<DAL.Entities.Post, GetPostModel>();
            CreateMap<CreatePostModel, DAL.Entities.Post>()
                .ForMember(u => u.Id, m => m.MapFrom(s => Guid.NewGuid()))
                .ForMember(u => u.CreationDate, m => m.MapFrom(s => DateTime.UtcNow));

            CreateMap<DAL.Entities.PostPhoto, GetPostPhotoModel>().AfterMap<PostPhotoToPostPhotoModelAction>();
        }
    }
}