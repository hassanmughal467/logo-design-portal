using AutoMapper;
using LogoDesignPortal.Application.DTOs.Auth;
using LogoDesignPortal.Application.DTOs.Orders;
using LogoDesignPortal.Application.DTOs.Users;
using LogoDesignPortal.Domain.Entities;
using LogoDesignPortal.Domain.Enums;

namespace LogoDesignPortal.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));

        // Order mappings
        CreateMap<LogoOrder, OrderResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Client, opt => opt.MapFrom(src => src.Client))
            .ForMember(dest => dest.Designer, opt => opt.MapFrom(src => src.Designer))
            .ForMember(dest => dest.FileCount, opt => opt.MapFrom(src => src.Files.Count));

        CreateMap<CreateOrderRequestDto, LogoOrder>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => OrderStatus.Pending))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Client Profile mappings
        CreateMap<ClientProfile, ClientInfoDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email));

        // Designer Profile mappings
        CreateMap<DesignerProfile, DesignerInfoDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName));
    }
}
