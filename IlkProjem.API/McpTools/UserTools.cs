// IlkProjem.API/McpTools/UserTools.cs
using System.ComponentModel;
using System.Text.Json;
using IlkProjem.BLL.Interfaces;
using ModelContextProtocol.Server;

namespace IlkProjem.API.McpTools;

[McpServerToolType]
public class UserTools(IUserService userService)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [McpServerTool, Description("Sistemdeki tüm kullanıcıları, personelleri veya üyeleri listeler. ID, kullanıcı adı, rol gibi bilgileri döner.")]
    public async Task<string> GetAllUsers()
    {
        var users = await userService.GetAllUsersAsync();

        return JsonSerializer.Serialize(new
        {
            success = true,
            data = users
        }, JsonOptions);
    }

    [McpServerTool, Description("Belirli bir kullanıcıyı veya personeli ID numarası ile sorgular, bilgilerini getirir.")]
    public async Task<string> GetUserById(
        [Description("Kullanıcı ID'si")] int id)
    {
        var user = await userService.GetUserByIdAsync(id);

        if (user == null)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                message = $"ID {id} ile kullanıcı bulunamadı."
            }, JsonOptions);
        }

        return JsonSerializer.Serialize(new
        {
            success = true,
            data = user
        }, JsonOptions);
    }

    [McpServerTool, Description("Kullanıcıyı Admin (Yönetici) yapar, rolünü yükseltir veya yetki verir.")]
    public async Task<string> PromoteUser(
        [Description("Admin'e yükseltilecek kullanıcı ID'si")] int id)
    {
        try
        {
            var user = await userService.PromoteUserAsync(id);

            return JsonSerializer.Serialize(new
            {
                success = true,
                message = $"Kullanıcı '{user.Username}' başarıyla Admin rolüne yükseltildi.",
                data = user
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                message = ex.Message
            }, JsonOptions);
        }
    }

    [McpServerTool, Description("Kullanıcının Admin (Yönetici) yetkisini geri alır, normal kullanıcı rolüne düşürür.")]
    public async Task<string> DemoteUser(
        [Description("User rolüne düşürülecek kullanıcı ID'si")] int id)
    {
        try
        {
            var user = await userService.DemoteUserAsync(id);

            return JsonSerializer.Serialize(new
            {
                success = true,
                message = $"Kullanıcı '{user.Username}' başarıyla User rolüne düşürüldü.",
                data = user
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                message = ex.Message
            }, JsonOptions);
        }
    }
}
