using IlkProjem.Core.Interfaces;
using IlkProjem.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IlkProjem.DAL.Interceptors;

/// <summary>
/// EF Core SaveChanges Interceptor.
/// BaseEntity türevlerindeki audit alanlarını (Created/Updated/Deleted)
/// otomatik olarak doldurur ve fiziksel silmeleri soft delete'e çevirir.
/// </summary>
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public AuditSaveChangesInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ProcessAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ProcessAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ProcessAuditFields(DbContext? context)
    {
        if (context is null) return;

        var userName = _currentUserService.UserName;
        var userId = _currentUserService.UserId;

        // Use .ToList() to prevent "Collection was modified" exceptions
        var entries = context.ChangeTracker.Entries<BaseEntity>().ToList();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = userName;
                    entry.Entity.CreatedByUserId = userId;
                    entry.Entity.IsActive = true;
                    entry.Entity.IsDeleted = false;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = userName;
                    entry.Entity.UpdatedByUserId = userId;
                    // CreatedAt/By asla değişmemeli
                    entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(BaseEntity.CreatedBy)).IsModified = false;
                    entry.Property(nameof(BaseEntity.CreatedByUserId)).IsModified = false;
                    break;

                case EntityState.Deleted:
                    // Fiziksel silme yerine soft delete
                    // DİKKAT: entry.State = EntityState.Modified yaparsak TÜM sütunlar update edilir!
                    // Bunun yerine Unchanged yapıp sadece sildiğimiz alanların güncellenmesini sağlıyoruz.
                    entry.State = EntityState.Unchanged;
                    
                    entry.Property(nameof(BaseEntity.IsDeleted)).CurrentValue = true;
                    entry.Property(nameof(BaseEntity.IsDeleted)).IsModified = true;

                    entry.Property(nameof(BaseEntity.IsActive)).CurrentValue = false;
                    entry.Property(nameof(BaseEntity.IsActive)).IsModified = true;

                    entry.Property(nameof(BaseEntity.DeletedAt)).CurrentValue = DateTime.UtcNow;
                    entry.Property(nameof(BaseEntity.DeletedAt)).IsModified = true;

                    entry.Property(nameof(BaseEntity.DeletedBy)).CurrentValue = userName;
                    entry.Property(nameof(BaseEntity.DeletedBy)).IsModified = true;

                    entry.Property(nameof(BaseEntity.DeletedByUserId)).CurrentValue = userId;
                    entry.Property(nameof(BaseEntity.DeletedByUserId)).IsModified = true;
                    break;
            }
        }
    }
}
