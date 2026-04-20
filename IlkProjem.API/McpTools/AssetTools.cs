// IlkProjem.API/McpTools/AssetTools.cs
using System.ComponentModel;
using System.Text.Json;
using IlkProjem.BLL.Interfaces;
using IlkProjem.Core.Dtos.CarDtos;
using IlkProjem.Core.Dtos.HouseDtos;
using ModelContextProtocol.Server;

namespace IlkProjem.API.McpTools;

[McpServerToolType]
public class AssetTools(ICarService carService, IHouseService houseService)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    // ==================== ARAÇ (CAR) TOOLS ====================

    [McpServerTool, Description("Belirli bir müşterinin üzerine kayıtlı tüm araçları, arabaları veya taşıtları listeler, getirir.")]
    public async Task<string> GetCarsByCustomerId(
        [Description("Araçları listelenecek müşteri ID'si")] int customerId)
    {
        var result = await carService.GetCarsByCustomerId(customerId);

        return JsonSerializer.Serialize(new
        {
            success = result.Success,
            message = result.Message,
            data = result.Data
        }, JsonOptions);
    }

    [McpServerTool, Description("Müşteriye ait yeni bir araç, araba veya plaka kaydı ekler/oluşturur.")]
    public async Task<string> AddCar(
        [Description("Aracın ait olduğu müşteri ID'si")] int customerId,
        [Description("Araç plakası (zorunlu, örn: '34 ABC 123')")] string plate,
        [Description("Araç açıklaması (örn: '2023 Model BMW X5')")] string? description = null)
    {
        var dto = new CarCreateDto
        {
            CustomerId = customerId,
            Plate = plate,
            Description = description
        };

        var result = await carService.AddCar(dto);

        return JsonSerializer.Serialize(new
        {
            success = result.Success,
            message = result.Message,
            data = result.Success ? new { carId = result.Data } : null
        }, JsonOptions);
    }

    [McpServerTool, Description("Mevcut araç bilgilerini günceller.")]
    public async Task<string> UpdateCar(
        [Description("Güncellenecek araç ID'si")] int id,
        [Description("Yeni araç plakası (zorunlu)")] string plate,
        [Description("Yeni araç açıklaması")] string? description = null)
    {
        var dto = new CarUpdateDto
        {
            Id = id,
            Plate = plate,
            Description = description
        };

        var result = await carService.UpdateCar(dto);

        return JsonSerializer.Serialize(new
        {
            success = result.Success,
            message = result.Message
        }, JsonOptions);
    }

    [McpServerTool, Description("Bir araç veya araba kaydını sistemden siler/kaldırır.")]
    public async Task<string> DeleteCar(
        [Description("Silinecek araç ID'si")] int id)
    {
        var result = await carService.DeleteCar(id);

        return JsonSerializer.Serialize(new
        {
            success = result.Success,
            message = result.Message
        }, JsonOptions);
    }

    // ==================== GAYRİMENKUL (HOUSE) TOOLS ====================

    [McpServerTool, Description("Belirli bir kişinin evlerini, konutlarını, taşınmazlarını veya gayrimenkullerini listeler.")]
    public async Task<string> GetHousesByCustomerId(
        [Description("Gayrimenkulleri listelenecek müşteri ID'si")] int customerId)
    {
        var result = await houseService.GetHousesByCustomerId(customerId);

        return JsonSerializer.Serialize(new
        {
            success = result.Success,
            message = result.Message,
            data = result.Data
        }, JsonOptions);
    }

    [McpServerTool, Description("Müşteri adına yeni bir ev, konut veya gayrimenkul kaydı açar/ekler.")]
    public async Task<string> AddHouse(
        [Description("Gayrimenkulün ait olduğu müşteri ID'si")] int customerId,
        [Description("Gayrimenkulün adresi (zorunlu)")] string address,
        [Description("Gayrimenkul açıklaması (örn: 'Deniz manzaralı 3+1 dubleks')")] string? description = null)
    {
        var dto = new HouseCreateDto
        {
            CustomerId = customerId,
            Address = address,
            Description = description
        };

        var result = await houseService.AddHouse(dto);

        return JsonSerializer.Serialize(new
        {
            success = result.Success,
            message = result.Message,
            data = result.Success ? new { houseId = result.Data } : null
        }, JsonOptions);
    }

    [McpServerTool, Description("Mevcut gayrimenkul bilgilerini günceller.")]
    public async Task<string> UpdateHouse(
        [Description("Güncellenecek gayrimenkul ID'si")] int id,
        [Description("Yeni adres (zorunlu)")] string address,
        [Description("Yeni açıklama")] string? description = null)
    {
        var dto = new HouseUpdateDto
        {
            Id = id,
            Address = address,
            Description = description
        };

        var result = await houseService.UpdateHouse(dto);

        return JsonSerializer.Serialize(new
        {
            success = result.Success,
            message = result.Message
        }, JsonOptions);
    }

    [McpServerTool, Description("Bir gayrimenkul, ev veya konut kaydını sistemden siler.")]
    public async Task<string> DeleteHouse(
        [Description("Silinecek gayrimenkul ID'si")] int id)
    {
        var result = await houseService.DeleteHouse(id);

        return JsonSerializer.Serialize(new
        {
            success = result.Success,
            message = result.Message
        }, JsonOptions);
    }
}
