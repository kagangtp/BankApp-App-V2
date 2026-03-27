using IlkProjem.BLL.Interfaces;
using IlkProjem.Core.Dtos.UserDtos;
using IlkProjem.Core.Exceptions;
using IlkProjem.DAL.Interfaces;

namespace IlkProjem.BLL.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken ct = default)
    {
        var users = await _userRepository.GetAllAsync(ct);
        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Role = u.Role,
            CreatedAt = u.CreatedAt
        });
    }

    public async Task<UserDto?> GetUserByIdAsync(int id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user == null) return null;

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserDto> UpdateUserAsync(int id, UserUpdateDto userUpdateDto, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user == null)
        {
            throw new BusinessException(IlkProjem.Core.Enums.BusinessErrorCode.UserNotFound, $"User with ID {id} not found.");
        }

        user.Username = userUpdateDto.Username;
        user.Email = userUpdateDto.Email;
        user.Role = userUpdateDto.Role;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(ct);

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<bool> DeleteUserAsync(int id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user == null) return false;

        _userRepository.Delete(user);
        return await _userRepository.SaveChangesAsync(ct);
    }

    public async Task<UserDto> PromoteUserAsync(int id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user == null)
            throw new BusinessException(IlkProjem.Core.Enums.BusinessErrorCode.UserNotFound, $"User with ID {id} not found.");

        // Staff -> Manager -> Admin
        if (string.IsNullOrEmpty(user.Role) || user.Role == IlkProjem.Core.Constants.Roles.Staff)
        {
            user.Role = IlkProjem.Core.Constants.Roles.Manager;
        }
        else if (user.Role == IlkProjem.Core.Constants.Roles.Manager)
        {
            user.Role = IlkProjem.Core.Constants.Roles.Admin;
        }

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(ct);

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserDto> DemoteUserAsync(int id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user == null)
            throw new BusinessException(IlkProjem.Core.Enums.BusinessErrorCode.UserNotFound, $"User with ID {id} not found.");

        // Admin -> Manager -> Staff
        if (user.Role == IlkProjem.Core.Constants.Roles.Admin)
        {
            user.Role = IlkProjem.Core.Constants.Roles.Manager;
        }
        else if (user.Role == IlkProjem.Core.Constants.Roles.Manager)
        {
            user.Role = IlkProjem.Core.Constants.Roles.Staff;
        }

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(ct);

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }
}
