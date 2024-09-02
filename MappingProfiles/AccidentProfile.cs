using AutoMapper;
using FleetPulse_BackEndDevelopment.DTOs;
using FleetPulse_BackEndDevelopment.Models;

namespace FleetPulse_BackEndDevelopment.MappingProfiles
{
    public class AccidentProfile : Profile
    {
        public AccidentProfile()
        {
            CreateMap<Accident, AccidentDTO>();
            CreateMap<AccidentCreateDTO, Accident>()
                .ForMember(dest => dest.Photos, opt => opt.Ignore()); // Handle Photos separately
        }
    }
}