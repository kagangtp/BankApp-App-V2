// IlkProjem.API/McpTools/CustomerTools.cs
using System.ComponentModel;
using System.Text.Json;
using IlkProjem.BLL.Interfaces;
using IlkProjem.Core.Dtos.CustomerDtos;
using IlkProjem.Core.Dtos.SpecificationDtos;
using ModelContextProtocol.Server;

namespace IlkProjem.API.McpTools;

[McpServerToolType]
public class CustomerTools(ICustomerService customerService)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [McpServerTool, Description("Müşteri listesini sayfalı olarak getirir. Arama ve sıralama desteği vardır.")]
    public async Task<string> GetCustomers(
        [Description("Sayfa boyutu (varsayılan 10, maksimum 50)")] int pageSize = 10,
        [Description("Cursor tabanlı sayfalama için son müşteri ID'si (ilk sayfa için 0)")] int lastId = 0,
        [Description("İsim veya email ile arama filtresi")] string? search = null)
    {
        var specParams = new CustomerSpecParams
        {
            PageSize = pageSize,
            LastId = lastId,
            Search = search
        };

        var result = await customerService.GetCustomersAsync(specParams);

        return JsonSerializer.Serialize(new
        {
            success = result.Success,
            message = result.Message,
            data = result.Data
        }, JsonOptions);
    }

    [McpServerTool, Description("Belirli bir müşteriyi ID ile getirir. Detaylı müşteri bilgilerini döner.")]
    public async Task<string> GetCustomerById(
        [Description("Müşteri ID'si")] int id)
    {
        var result = await customerService.GetCustomerById(id);

        return JsonSerializer.Serialize(new
        {
            success = result.Success,
            message = result.Message,
            data = result.Data
        }, JsonOptions);
    }

    [McpServerTool, Description("Yeni müşteri oluşturur. İsim zorunludur.")]
    public async Task<string> AddCustomer(
        [Description("Müşteri adı soyadı (zorunlu)")] string name,
        [Description("Müşteri email adresi")] string? email = null,
        [Description("Doğum tarihi (YYYY-MM-DD formatında)")] string? birthDate = null)
    {
        var dto = new CustomerCreateDto
        {
            Name = name,
            Email = email,
            BirthDate = birthDate != null ? DateTime.Parse(birthDate) : null
        };

        var result = await customerService.AddCustomer(dto);

        return JsonSerializer.Serialize(new
        {
            success = result.Success,
            message = result.Message
        }, JsonOptions);
    }

    [McpServerTool, Description("Mevcut müşteri bilgilerini günceller. Güncellenecek müşterinin ID'si zorunludur.")]
    public async Task<string> UpdateCustomer(
        [Description("Güncellenecek müşteri ID'si")] int id,
        [Description("Yeni müşteri adı soyadı (zorunlu)")] string name,
        [Description("Yeni email adresi")] string? email = null,
        [Description("Yeni bakiye tutarı")] decimal balance = 0,
        [Description("TC Kimlik Numarası")] string? tcKimlikNo = null,
        [Description("Doğum tarihi (YYYY-MM-DD formatında)")] string? birthDate = null)
    {
        var dto = new CustomerUpdateDto
        {
            Id = id,
            Name = name,
            Email = email,
            Balance = balance,
            TcKimlikNo = tcKimlikNo,
            BirthDate = birthDate != null ? DateTime.Parse(birthDate) : null
        };

        var result = await customerService.UpdateCustomer(dto);

        return JsonSerializer.Serialize(new
        {
            success = result.Success,
            message = result.Message
        }, JsonOptions);
    }

    [McpServerTool, Description("Müşteriyi sistemden siler. Bu işlem geri alınamaz.")]
    public async Task<string> DeleteCustomer(
        [Description("Silinecek müşteri ID'si")] int id)
    {
        var dto = new CustomerDeleteDto { Id = id };
        var result = await customerService.DeleteCustomer(dto);

        return JsonSerializer.Serialize(new
        {
            success = result.Success,
            message = result.Message
        }, JsonOptions);
    }
}
