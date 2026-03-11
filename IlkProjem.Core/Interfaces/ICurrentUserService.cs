namespace IlkProjem.Core.Interfaces;
public interface ICurrentUserService
{
    string? UserName { get; }
    int UserId { get; }
}