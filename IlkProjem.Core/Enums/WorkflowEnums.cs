namespace IlkProjem.Core.Enums;

/// <summary>
/// Workflow işlem türleri.
/// </summary>
public enum WorkflowType
{
    CustomerDelete,      // Müşteri Silme
    CustomerUpdate       // Müşteri Güncelleme
}

/// <summary>
/// Workflow durum makinesi durumları.
/// DRAFT → PENDING → MANAGER_APPROVAL → APPROVED / REJECTED → COMPLETED
/// </summary>
public enum WorkflowStatus
{
    Draft,              // Taslak / Düzenlenebilir
    Pending,            // Onay Bekliyor
    ManagerApproval,    // Müdür Onay aşamasında
    Approved,           // Onaylandı — İş aksiyonu hazır
    Rejected,           // Reddedildi
    Completed,          // Tamamlandı (iş aksiyonu çalıştı)
    Cancelled           // İptal Edildi
}

/// <summary>
/// Workflow üzerinde yapılan aksiyon türleri.
/// </summary>
public enum WorkflowActionType
{
    Approve,
    Reject,
    Comment,
    Cancel
}
