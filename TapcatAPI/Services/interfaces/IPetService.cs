using TapcatAPI.DTOs;

namespace TapcatAPI.Services;

public interface IPetService
{
    Task<IEnumerable<PetDTO>> GetAll();
    Task<PetDTO?> GetById(int id);
    Task<PetDTO> Create(CreatePetDTO dto);
    Task Update(int id, UpdatePetDTO dto);
    Task Delete(int id);
}