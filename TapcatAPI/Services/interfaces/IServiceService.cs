using TapcatAPI.DTOs;

namespace TapcatAPI.Services;

public interface IServiceService
{
    Task<IEnumerable<ServiceDTO>> GetAll();
    Task<ServiceDTO?> GetById(int id);
    Task<ServiceDTO> Create(CreateServiceDTO dto);
    Task Update(int id, UpdateServiceDTO dto);
    Task Delete(int id);
}