using IlkProjem.Core.Enums;
using IlkProjem.Core.Models;
using IlkProjem.DAL.Data;
using IlkProjem.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IlkProjem.DAL.Repositories;

public class WorkflowRepository : GenericRepository<Workflow>, IWorkflowRepository
{
    public WorkflowRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Workflow?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
    {
        return await _context.Workflows
            .Include(w => w.RequestedByUser)
            .Include(w => w.AssignedToUser)
            .Include(w => w.Steps)
                .ThenInclude(s => s.AssignedToUser)
            .Include(w => w.Actions)
                .ThenInclude(a => a.ByUser)
            .Include(w => w.History)
            .AsSplitQuery()
            .FirstOrDefaultAsync(w => w.Id == id, ct);
    }

    public async Task<List<Workflow>> GetPendingByManagerAsync(int userId, CancellationToken ct = default)
    {
        return await _context.Workflows
            .Include(w => w.RequestedByUser)
            .Where(w => w.AssignedToUserId == userId 
                     && (w.Status == WorkflowStatus.Pending || w.Status == WorkflowStatus.ManagerApproval))
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Workflow?> GetByWorkflowNoAsync(string workflowNo, CancellationToken ct = default)
    {
        return await _context.Workflows
            .Include(w => w.RequestedByUser)
            .FirstOrDefaultAsync(w => w.WorkflowNo == workflowNo, ct);
    }

    public async Task<List<Workflow>> GetAllWithDetailsAsync(WorkflowStatus? statusFilter = null, CancellationToken ct = default)
    {
        var query = _context.Workflows
            .Include(w => w.RequestedByUser)
            .Include(w => w.AssignedToUser)
            .AsQueryable();

        if (statusFilter.HasValue)
        {
            query = query.Where(w => w.Status == statusFilter.Value);
        }

        return await query
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<WorkflowStatus, int>> GetStatusCountsAsync(CancellationToken ct = default)
    {
        return await _context.Workflows
            .GroupBy(w => w.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, ct);
    }

    public async Task<string> GenerateWorkflowNoAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"WF-{year}-";
        
        var lastNo = await _context.Workflows
            .Where(w => w.WorkflowNo.StartsWith(prefix))
            .OrderByDescending(w => w.WorkflowNo)
            .Select(w => w.WorkflowNo)
            .FirstOrDefaultAsync(ct);

        int nextNumber = 1;
        if (lastNo != null)
        {
            var numberPart = lastNo.Replace(prefix, "");
            if (int.TryParse(numberPart, out var lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}";
    }
}
