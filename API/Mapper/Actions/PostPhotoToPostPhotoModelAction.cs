using API.Models.Attach;
using API.Services;
using AutoMapper;
using DAL.Entities;

namespace API.Mapper.Actions
{
    public class PostPhotoToPostPhotoModelAction : IMappingAction<PostPhoto, GetPostPhotoModel>
    {
        private LinkProviderService _linkService;
        public PostPhotoToPostPhotoModelAction(LinkProviderService linkService)
        {
            _linkService = linkService;
        }
        public void Process(PostPhoto source, GetPostPhotoModel destination, ResolutionContext context) =>
            _linkService.InjectPostPhotoURL(source, destination);

    }
}
