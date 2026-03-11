namespace IlkProjem.Core.Dtos.SpecificationDtos;
public class CustomerSpecParams
{
    // Use a constant to prevent someone from asking for 1 million records at once
    private const int MaxPageSize = 50;
        
    // Match these exactly with your Angular interface
    public string? Search { get; set; }
    public string? Sort { get; set; }
        
    public int LastId { get; set; } = 0; // Default to 0 for the first page

    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
 }