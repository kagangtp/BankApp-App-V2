using IlkProjem.Core.Models;
using IlkProjem.DAL.Data;
using IlkProjem.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace IlkProjem.DAL.Repositories;
public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default) => 
        await _context.Users.AnyAsync(u => u.Email == email, ct);
}
