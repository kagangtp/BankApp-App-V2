using IlkProjem.Core.Specifications;
using IlkProjem.DAL.Data;
using IlkProjem.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IlkProjem.DAL.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AppDbContext _context;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Set<T>().FindAsync(new object[] { id }, ct);
    }

    public virtual async Task<T?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return await _context.Set<T>().FindAsync(new object[] { id }, ct);
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<T>().FindAsync(new object[] { id }, ct);
    }

    public virtual async Task<List<T>> ListAllAsync(CancellationToken ct = default)
    {
        return await _context.Set<T>().ToListAsync(ct);
    }

    public virtual async Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        return await ApplySpecification(spec).ToListAsync(ct);
    }

    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await _context.Set<T>().AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
    }

    public virtual async Task<bool> UpdateAsync(T entity, CancellationToken ct = default)
    {
        // Entity Framework tracks changes, so we mark it as modified if it's not already tracked.
        // If it's already tracked (e.g. fetched via GetById), SaveChangesAsync handles it.
        _context.Entry(entity).State = EntityState.Modified;
        
        int affectedRows = await _context.SaveChangesAsync(ct);
        return affectedRows > 0;
    }

    public virtual async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _context.Set<T>().FindAsync(new object[] { id }, ct);
        if (entity == null) return false;

        _context.Set<T>().Remove(entity);
        int affectedRows = await _context.SaveChangesAsync(ct);
        return affectedRows > 0;
    }

    public virtual async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await _context.Set<T>().FindAsync(new object[] { id }, ct);
        if (entity == null) return false;

        _context.Set<T>().Remove(entity);
        int affectedRows = await _context.SaveChangesAsync(ct);
        return affectedRows > 0;
    }

    public virtual async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.Set<T>().FindAsync(new object[] { id }, ct);
        if (entity == null) return false;

        _context.Set<T>().Remove(entity);
        int affectedRows = await _context.SaveChangesAsync(ct);
        return affectedRows > 0;
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        return SpecificationEvaluator<T>.GetQuery(_context.Set<T>().AsQueryable(), spec);
    }
}
