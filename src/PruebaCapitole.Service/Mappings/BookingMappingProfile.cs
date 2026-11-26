using AutoMapper;
using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Service.Bookings.Queries;

namespace PruebaCapitole.Service.Mappings
{
    /// <summary>Reglas de AutoMapper: de la entidad Booking a los DTOs de lectura.</summary>
    public sealed class BookingMappingProfile : Profile
    {
        public BookingMappingProfile()
        {
            CreateMap<Booking, BookingListItemDto>()
                .ForCtorParam(nameof(BookingListItemDto.Id), o => o.MapFrom(b => b.Id.Value))
                .ForCtorParam(nameof(BookingListItemDto.ResourceId), o => o.MapFrom(b => b.ResourceId.Value))
                .ForCtorParam(nameof(BookingListItemDto.CustomerName), o => o.MapFrom(b => b.Customer.Name))
                .ForCtorParam(nameof(BookingListItemDto.CustomerEmail), o => o.MapFrom(b => b.Customer.Email))
                .ForCtorParam(nameof(BookingListItemDto.Start), o => o.MapFrom(b => b.DateRange.Start))
                .ForCtorParam(nameof(BookingListItemDto.End), o => o.MapFrom(b => b.DateRange.End))
                .ForCtorParam(nameof(BookingListItemDto.Status), o => o.MapFrom(b => b.Status.ToString()));

            CreateMap<Booking, BookingDetailsDto>()
                .ForCtorParam(nameof(BookingDetailsDto.Id), o => o.MapFrom(b => b.Id.Value))
                .ForCtorParam(nameof(BookingDetailsDto.ResourceId), o => o.MapFrom(b => b.ResourceId.Value))
                .ForCtorParam(nameof(BookingDetailsDto.CustomerName), o => o.MapFrom(b => b.Customer.Name))
                .ForCtorParam(nameof(BookingDetailsDto.CustomerEmail), o => o.MapFrom(b => b.Customer.Email))
                .ForCtorParam(nameof(BookingDetailsDto.Start), o => o.MapFrom(b => b.DateRange.Start))
                .ForCtorParam(nameof(BookingDetailsDto.End), o => o.MapFrom(b => b.DateRange.End))
                .ForCtorParam(nameof(BookingDetailsDto.Status), o => o.MapFrom(b => b.Status.ToString()));
        }
    }
}
