using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using IlkProjem.DAL.Data;
using IlkProjem.Core.Utilities.Results;

namespace IlkProjem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuditController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Tüm audit loglarını son eklenen en başta olacak şekilde listeler.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? entityName,
        [FromQuery] string? action,
        [FromQuery] int pageSize = 50,
        [FromQuery] int page = 0,
        CancellationToken ct = default)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityName))
            query = query.Where(a => a.EntityName == entityName);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action);

        var total = await query.CountAsync(ct);

        var logs = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip(page * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.Id,
                a.WorkflowId,
                a.EntityName,
                a.Action,
                a.OldValue,
                a.NewValue,
                a.ChangedByUserId,
                a.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(new SuccessDataResult<object>(new { logs, total }));
    }
}
