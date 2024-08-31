using AutoMapper;
using FleetPulse_BackEndDevelopment.DTOs;
using FleetPulse_BackEndDevelopment.Models;

namespace FleetPulse_BackEndDevelopment.MappingProfiles
{
    public class AccidentProfile : Profile
    {
        public AccidentProfile()
        {
            // Map from DTO to Entity
            CreateMap<AccidentDTO, Accident>()
                .ForMember(dest => dest.VehicleId, opt => opt.Ignore()) // Ignore VehicleId mapping
                .ForMember(dest => dest.UserId, opt => opt.Ignore());   // Ignore UserId mapping
            CreateMap<Accident, AccidentDTO>()
                .ForMember(dest => dest.VehicleRegistrationNo, opt => opt.MapFrom(src => src.Vehicle.VehicleRegistrationNo))
                .ForMember(dest => dest.NIC, opt => opt.MapFrom(src => src.User.NIC));
            

            // Map from Entity to DTO
            CreateMap<Accident, AccidentDTO>()
                .ForMember(dest => dest.VehicleRegistrationNo, opt => opt.MapFrom(src => src.Vehicle.VehicleRegistrationNo))
                .ForMember(dest => dest.NIC, opt => opt.MapFrom(src => src.User.NIC));
            
        }
    }
}