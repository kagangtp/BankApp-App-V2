using ClosedXML.Excel;
using IlkProjem.BLL.Interfaces;
using System.Reflection;

namespace IlkProjem.BLL.Services;

public class ExcelService : IExcelService
{
    public byte[] GenerateExcel<T>(IEnumerable<T> data, string sheetName = "Sayfa1")
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);
        
        // T tipinin public property'lerini alıyoruz (Reflection)
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // 1. BAŞLIKLARI OLUŞTUR (Property isimlerinden)
        for (int i = 0; i < properties.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = properties[i].Name;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        // 2. VERİLERİ DOLDUR
        var list = data.ToList();
        for (int rowIndex = 0; rowIndex < list.Count; rowIndex++)
        {
            for (int colIndex = 0; colIndex < properties.Length; colIndex++)
            {
                // Değeri al ve hücreye yaz
                var value = properties[colIndex].GetValue(list[rowIndex]);
                worksheet.Cell(rowIndex + 2, colIndex + 1).Value = value?.ToString() ?? "";
            }
        }

        // 3. FORMATLAMA
        worksheet.Columns().AdjustToContents(); // Sütun genişliklerini ayarla

        // 4. STREAM OLARAK DÖN
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}