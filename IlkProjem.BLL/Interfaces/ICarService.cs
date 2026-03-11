using IlkProjem.Core.Dtos.CarDtos;
using IlkProjem.Core.Utilities.Results;

namespace IlkProjem.BLL.Interfaces;

public interface ICarService
{
    Task<IDataResult<int>> AddCar(CarCreateDto createDto, CancellationToken ct = default);
    Task<IDataResult<List<CarReadDto>>> GetCarsByCustomerId(int customerId, CancellationToken ct = default);
    Task<IDataResult<CarReadDto>> GetCarById(int id, CancellationToken ct = default);
    Task<IResult> UpdateCar(CarUpdateDto updateDto, CancellationToken ct = default);
    Task<IResult> DeleteCar(int id, CancellationToken ct = default);
}
