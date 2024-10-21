using AutoMapper;
using SWKOM_DMS.Entities;
using SWKOM_DMS.DTOs;

namespace SWKOM_DMS
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Document, DocumentDto>().ReverseMap();
        }
    }
}
