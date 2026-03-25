namespace IlkProjem.Core.Constants;

/// <summary>
/// Authorization policy isimleri.
/// Program.cs'te AddAuthorization() içinde tanımlanır,
/// Controller'larda [Authorize(Policy = "...")] ile kullanılır.
/// </summary>
public static class Policies
{
    /// <summary>Admin + Manager: Müşteri / Araç / Ev ekleme, güncelleme, silme</summary>
    public const string CustomerManagement = "CustomerManagement";

    /// <summary>Sadece Admin: Kullanıcı yönetimi, silme işlemleri</summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>Sadece Admin: Kullanıcı CRUD işlemleri</summary>
    public const string UserManagement = "UserManagement";

    /// <summary>Admin + Manager: Dosya yükleme, silme</summary>
    public const string FileManagement = "FileManagement";
}
