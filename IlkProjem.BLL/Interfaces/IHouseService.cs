using IlkProjem.Core.Dtos.HouseDtos;
using IlkProjem.Core.Utilities.Results;

namespace IlkProjem.BLL.Interfaces;

public interface IHouseService
{
    Task<IDataResult<int>> AddHouse(HouseCreateDto createDto, CancellationToken ct = default);
    Task<IDataResult<List<HouseReadDto>>> GetHousesByCustomerId(int customerId, CancellationToken ct = default);
    Task<IDataResult<HouseReadDto>> GetHouseById(int id, CancellationToken ct = default);
    Task<IResult> UpdateHouse(HouseUpdateDto updateDto, CancellationToken ct = default);
    Task<IResult> DeleteHouse(int id, CancellationToken ct = default);
}
