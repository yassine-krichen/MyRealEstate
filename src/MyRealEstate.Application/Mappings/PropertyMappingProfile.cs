using AutoMapper;
using MyRealEstate.Application.DTOs;
using MyRealEstate.Domain.Entities;
using MyRealEstate.Domain.ValueObjects;

namespace MyRealEstate.Application.Mappings;

public class PropertyMappingProfile : Profile
{
    public PropertyMappingProfile()
    {
        // Property -> PropertyListDto
        CreateMap<Property, PropertyListDto>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Price.Currency))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Address.City))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.MainImageUrl, opt => opt.Ignore()); // Handled manually in query
        
        // Property -> PropertyDetailDto
        CreateMap<Property, PropertyDetailDto>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Price.Currency))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ViewCount, opt => opt.Ignore()); // Handled manually in query
        
        // Address -> AddressDto
        CreateMap<Address, AddressDto>();
        
        // User -> AgentDto
        CreateMap<User, AgentDto>();
        
        // PropertyImage -> PropertyImageDto
        CreateMap<PropertyImage, PropertyImageDto>()
            .ForMember(dest => dest.Url, opt => opt.Ignore()); // Handled manually in query
    }
}
