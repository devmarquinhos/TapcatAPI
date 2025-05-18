using AutoMapper;
using TapcatAPI.Models;
using TapcatAPI.DTOs;

namespace TapcatAPI.Profiles;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Service, ServiceDTO>().ReverseMap();
        CreateMap<CreateServiceDTO, Service>().ReverseMap();
        CreateMap<UpdateServiceDTO, Service>().ReverseMap();
        
        CreateMap<Customer, CustomerDTO>().ReverseMap();
        CreateMap<CreateCustomerDTO, Customer>().ReverseMap();
        CreateMap<UpdateCustomerDTO, Customer>().ReverseMap();
        
        CreateMap<Pet, PetDTO>().ReverseMap();
        CreateMap<CreatePetDTO, Pet>().ReverseMap();
        CreateMap<UpdatePetDTO, Pet>().ReverseMap();
        
        CreateMap<Appointment, AppointmentDTO>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Pet.Customer.Name))
            .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.AppointmentServices.Select(a => a.Service)))
            .ReverseMap();
        CreateMap<CreateAppointmentDTO, Appointment>().ReverseMap();
        CreateMap<UpdateAppointmentDTO, Appointment>().ReverseMap();
        
        CreateMap<AppointmentService, AppointmentServiceDTO>().ReverseMap();
        CreateMap<CreateAppointmentServiceDTO, AppointmentService>().ReverseMap();
    }
}