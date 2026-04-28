using IlkProjem.Core.Models;
using IlkProjem.Core.Specifications;
using IlkProjem.DAL.Data;
using IlkProjem.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IlkProjem.DAL.Repositories;

public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Customer>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Customers.ToListAsync(ct);
    }

    // Override GetById to include profile image
    public override async Task<Customer?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.Customers
            .Include(c => c.ProfileImage)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public override async Task<bool> UpdateAsync(Customer customer, CancellationToken ct = default)
    {
        // Special case: we don't call Update(customer) to avoid breaking tracked navigation properties
        int affectedRows = await _context.SaveChangesAsync(ct);
        return affectedRows > 0;
    }
}