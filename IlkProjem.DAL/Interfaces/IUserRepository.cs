using IlkProjem.Core.Models;

namespace IlkProjem.DAL.Interfaces;

public interface IUserRepository
{
  // Giriş için kullanıcıyı e-postasıyla bulur
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default);
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    void Update(User user);
    void Delete(User user);
    Task<bool> SaveChangesAsync(CancellationToken ct = default);
}