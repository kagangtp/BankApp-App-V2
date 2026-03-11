namespace IlkProjem.BLL.Interfaces;

public interface ICalculatorService
{
    decimal Sum(decimal a, decimal b);
    decimal Difference(decimal a, decimal b);
    decimal Multiply(decimal a, decimal b);
    decimal Divide(decimal a, decimal b);
    Task<int> GetTotalAccountCountAsync();
    Task<decimal> GetTotalBalanceSumAsync();
}