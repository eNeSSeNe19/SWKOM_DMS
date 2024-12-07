//using AutoMapper;
//using SWKOM_DMS.Entities;
//using SWKOM_DMS.DTOs;

//namespace SWKOM_DMS
//{
//    public class MappingProfile : Profile
//    {
//        public MappingProfile()
//        {
//            CreateMap<Document, DocumentDto>().ReverseMap();
//        }
//    }
//}

using AutoMapper;
using SWKOM_DMS.Entities;
using SWKOM_DMS.DTOs;
using System;

namespace SWKOM_DMS
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<DocumentDto, Document>()
                .ForMember(dest => dest.FileContent,
                           opt => opt.MapFrom(src => Convert.FromBase64String(src.FileContent))) // Map and convert FileContent
                .ForMember(dest => dest.FileSize,
                           opt => opt.MapFrom(src => Convert.FromBase64String(src.FileContent).Length)) // Calculate FileSize
                .ForMember(dest => dest.UploadDate,
                           opt => opt.MapFrom(src => DateTime.UtcNow)); // Set UploadDate to current UTC time

            CreateMap<Document, DocumentDto>()
                .ForMember(dest => dest.FileContent,
                           opt => opt.MapFrom(src => Convert.ToBase64String(src.FileContent))); // Convert FileContent back to base64
        }
    }
}
