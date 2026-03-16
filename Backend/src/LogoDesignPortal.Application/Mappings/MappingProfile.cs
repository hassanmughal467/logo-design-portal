using AutoMapper;
using LogoDesignPortal.Application.DTOs.Auth;
using LogoDesignPortal.Application.DTOs.Comments;
using LogoDesignPortal.Application.DTOs.Files;
using LogoDesignPortal.Application.DTOs.Gallery;
using LogoDesignPortal.Application.DTOs.Notifications;
using LogoDesignPortal.Application.DTOs.Orders;
using LogoDesignPortal.Application.DTOs.Payments;
using LogoDesignPortal.Application.DTOs.Revisions;
using LogoDesignPortal.Application.DTOs.Users;
using LogoDesignPortal.Application.Helpers;
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
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.ClientPrice, opt => opt.MapFrom(src => src.ClientPrice))
            .ForMember(dest => dest.ClientBasePrice, opt => opt.MapFrom(src => src.ClientBasePrice))
            .ForMember(dest => dest.ClientChargePrice, opt => opt.MapFrom(src => src.ClientChargePrice))
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode))
            .ForMember(dest => dest.DesignerProposedPrice, opt => opt.MapFrom(src => src.DesignerProposedPrice ?? src.ProposedPrice))
            .ForMember(dest => dest.DesignerApprovedPrice, opt => opt.MapFrom(src => src.DesignerApprovedPrice ?? src.ApprovedPrice))
            .ForMember(dest => dest.PriceUpdatedByRole, opt => opt.MapFrom(src => src.PriceUpdatedByRole))
            .ForMember(dest => dest.PriceUpdatedAt, opt => opt.MapFrom(src => src.PriceUpdatedAt))
            .ForMember(dest => dest.Client, opt => opt.MapFrom(src => src.Client))
            .ForMember(dest => dest.Designer, opt => opt.MapFrom(src => src.Designer))
            .ForMember(dest => dest.FileCount, opt => opt.MapFrom(src => src.Files != null ? src.Files.Count : 0))
            .ForMember(dest => dest.VisibleFileCount, opt => opt.MapFrom(src => src.Files != null ? src.Files.Count(f => f.IsVisibleToClient) : 0))
            .ForMember(dest => dest.RevisionCount, opt => opt.MapFrom(src => src.Revisions != null ? src.Revisions.Count : 0))
            .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments != null ? src.Comments.Count : 0));

        CreateMap<CreateOrderRequestDto, LogoOrder>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => OrderStatus.WaitingForAdminApproval))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority ?? OrderPriority.Medium))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<UpdateOrderRequestDto, LogoOrder>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.ClientId, opt => opt.Ignore())
            .ForMember(dest => dest.DesignerId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

        // Client Profile mappings
        CreateMap<ClientProfile, ClientInfoDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName));

        // Designer Profile mappings
        CreateMap<DesignerProfile, DesignerInfoDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName));

        // Notification mappings - build RedirectUrl when null (e.g. legacy notifications)
        CreateMap<Notification, NotificationResponseDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.ReferenceType, opt => opt.MapFrom(src => src.ReferenceType.ToString()))
            .ForMember(dest => dest.ReferenceId, opt => opt.MapFrom(src => src.ReferenceId ?? src.OrderId))
            .ForMember(dest => dest.RedirectUrl, opt => opt.MapFrom(src =>
                !string.IsNullOrEmpty(src.RedirectUrl)
                    ? src.RedirectUrl
                    : NotificationRedirectHelper.BuildRedirectUrl(src.ReferenceType, src.ReferenceId ?? src.OrderId, src.OrderId)));

        // Comment mappings
        CreateMap<OrderComment, CommentResponseDto>()
            .ForMember(dest => dest.CreatedByName, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByRole, opt => opt.Ignore());

        // Revision mappings
        CreateMap<OrderRevision, RevisionResponseDto>()
            .ForMember(dest => dest.RequestedByName, opt => opt.Ignore())
            .ForMember(dest => dest.Files, opt => opt.Ignore());

        // Gallery mappings
        CreateMap<ClientGallery, GalleryItemResponseDto>()
            .ForMember(dest => dest.OrderTitle, opt => opt.MapFrom(src => src.Order.Title));

        // File mappings
        CreateMap<LogoFile, FileResponseDto>()
            .ForMember(dest => dest.FileType, opt => opt.MapFrom(src => src.FileType.ToString()))
            .ForMember(dest => dest.UploadedByName, opt => opt.Ignore())
            .ForMember(dest => dest.ApprovedByName, opt => opt.Ignore());

        // Payment mappings
        CreateMap<Payment, PaymentResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.Invoice.InvoiceNumber));
    }
}
