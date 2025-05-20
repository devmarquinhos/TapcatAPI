using TapcatAPI.DTOs;

namespace TapcatAPI.Services;

public interface IAppointmentService
{
    Task<IEnumerable<AppointmentDTO>> GetAll();
    Task<AppointmentDTO?> GetById(int id);
    Task<AppointmentDTO> Create(CreateAppointmentDTO dto);
    Task Update(int id, UpdateAppointmentDTO dto);
    Task Delete(int id);
}