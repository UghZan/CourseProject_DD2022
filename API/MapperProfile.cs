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

            CreateMap<DAL.Entities.Attach, Models.AttachModel>();
        }
    }
}
