using IlkProjem.Core.Dtos.UserDtos;

namespace IlkProjem.BLL.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken ct = default);
    Task<UserDto?> GetUserByIdAsync(int id, CancellationToken ct = default);
    Task<UserDto> UpdateUserAsync(int id, UserUpdateDto userUpdateDto, CancellationToken ct = default);
    Task<bool> DeleteUserAsync(int id, CancellationToken ct = default);
}
