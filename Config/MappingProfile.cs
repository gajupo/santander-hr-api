using AutoMapper;
using santander_hr_api.Models;

namespace santander_hr_api.Config
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Story, StoryDto>()
                .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Kids.Count));
        }
    }
}
