// IlkProjem.API/McpTools/DashboardTools.cs
using System.ComponentModel;
using System.Text.Json;
using IlkProjem.BLL.Interfaces;
using ModelContextProtocol.Server;

namespace IlkProjem.API.McpTools;

[McpServerToolType]
public class DashboardTools(ICalculatorService calculatorService)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [McpServerTool, Description("Sistemdeki toplam müşteri sayısını, üye sayısını veya kaç kişi olduğunu söyler.")]
    public async Task<string> GetTotalCustomerCount()
    {
        var count = await calculatorService.GetTotalAccountCountAsync();

        return JsonSerializer.Serialize(new
        {
            success = true,
            data = new { totalCustomers = count }
        }, JsonOptions);
    }

    [McpServerTool, Description("Bankadaki tüm müşterilerin toplam bakiye tutarını, kasanın genel durumunu veya toplam para miktarını söyler.")]
    public async Task<string> GetTotalBalance()
    {
        var total = await calculatorService.GetTotalBalanceSumAsync();

        return JsonSerializer.Serialize(new
        {
            success = true,
            data = new { totalBalance = total, currency = "TRY" }
        }, JsonOptions);
    }

    [McpServerTool, Description("Müşteri kayıt istatistiklerini, büyüme rakamlarını veya son aylarda kaç kişinin kayıt olduğunu gösterir. Dashboard grafikleri için veridir.")]
    public async Task<string> GetMonthlyRegistrations(
        [Description("Kaç aylık istatistik getirilsin (varsayılan 6, maksimum 12)")] int months = 6)
    {
        if (months > 12) months = 12;
        if (months < 1) months = 1;

        var data = await calculatorService.GetMonthlyRegistrationCountsAsync(months);

        return JsonSerializer.Serialize(new
        {
            success = true,
            data = data
        }, JsonOptions);
    }

    [McpServerTool, Description("Toplama, çıkarma, çarpma ve bölme gibi temel matematiksel işlemleri/hesaplamaları yapar.")]
    public string Calculate(
        [Description("Birinci sayı")] decimal a,
        [Description("İkinci sayı")] decimal b,
        [Description("İşlem türü: 'add' (toplama), 'subtract' (çıkarma), 'multiply' (çarpma), 'divide' (bölme)")] string operation)
    {
        try
        {
            decimal result = operation.ToLowerInvariant() switch
            {
                "add" or "toplama" => calculatorService.Sum(a, b),
                "subtract" or "çıkarma" => calculatorService.Difference(a, b),
                "multiply" or "çarpma" => calculatorService.Multiply(a, b),
                "divide" or "bölme" => calculatorService.Divide(a, b),
                _ => throw new ArgumentException($"Bilinmeyen işlem: {operation}. Desteklenen: add, subtract, multiply, divide")
            };

            return JsonSerializer.Serialize(new
            {
                success = true,
                data = new { a, b, operation, result }
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
