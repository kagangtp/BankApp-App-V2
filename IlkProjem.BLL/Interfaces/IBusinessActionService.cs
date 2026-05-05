using IlkProjem.Core.Models;
using IlkProjem.Core.Utilities.Results;

namespace IlkProjem.BLL.Interfaces;

/// <summary>
/// Onay sonrası gerçek iş aksiyonlarını çalıştıran servis.
/// Workflow onaylandığında CustomerDelete/CustomerUpdate gibi işlemleri tetikler.
/// </summary>
public interface IBusinessActionService
{
    Task<IResult> ExecuteAsync(Workflow workflow, CancellationToken ct = default);
}
