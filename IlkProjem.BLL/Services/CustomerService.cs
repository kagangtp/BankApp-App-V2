using IlkProjem.DAL.Interfaces;
using IlkProjem.Core.Models;
using IlkProjem.Core.Dtos.CustomerDtos;
using IlkProjem.BLL.Interfaces;
using IlkProjem.Core.Utilities.Results;
using Microsoft.Extensions.Localization;
using IlkProjem.Core.Resources;
using IlkProjem.Core.Dtos.SpecificationDtos;
using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using IlkProjem.Core.Hubs;
using IlkProjem.Core.Interfaces;
using Microsoft.Extensions.Caching.Hybrid; // 1. Yeni namespace

namespace IlkProjem.BLL.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly IValidator<CustomerCreateDto> _createValidator;
    private readonly IValidator<CustomerUpdateDto> _updateValidator;
    private readonly IValidator<CustomerDeleteDto> _deleteValidator;
    private readonly IFilesService _filesService;
    private readonly ICurrentUserService _currentUserService;
    private readonly HybridCache _cache; // 2. Cache field

    public CustomerService(
        ICustomerRepository repository,
        IStringLocalizer<Messages> localizer,
        IValidator<CustomerCreateDto> createValidator,
        IValidator<CustomerUpdateDto> updateValidator,
        IValidator<CustomerDeleteDto> deleteValidator,
        IFilesService filesService,
        IHubContext<NotificationHub> hubContext,
        ICurrentUserService currentUserService,
        HybridCache cache) // 3. DI Injection
    {
        _repository = repository;
        _localizer = localizer;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _deleteValidator = deleteValidator;
        _filesService = filesService;
        _hubContext = hubContext;
        _currentUserService = currentUserService;
        _cache = cache;
    }

    public async Task<IResult> AddCustomer(CustomerCreateDto createDto, CancellationToken ct = default)
    {
        var validationResult = await _createValidator.ValidateAsync(createDto, ct);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return new ErrorResult(errors);
        }

        var customer = new Customer 
        { 
            Name = createDto.Name, 
            Email = createDto.Email,
            BirthDate = createDto.BirthDate.HasValue 
                ? DateTime.SpecifyKind(createDto.BirthDate.Value, DateTimeKind.Utc) 
                : null
        };
        await _repository.AddAsync(customer, ct);

        // --- CACHE INVALIDATION ---
        // Yeni müşteri eklenince listeler değişeceği için genel listeyi siliyoruz
        await _cache.RemoveByTagAsync("customers_list", ct);

        // --- SIGNALR BİLDİRİMİ ---
        var userName = _currentUserService.UserName ?? "Sistem";
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", new {
            User = userName,
            Action = "CustomerCreated",
            Message = _localizer["CustomerCreatedNotification", userName, createDto.Name].Value
        }, ct);
        
        return new SuccessResult(_localizer["CustomerAdded"]); 
    }

    public async Task<IDataResult<List<CustomerReadDto>>> GetCustomersAsync(CustomerSpecParams custParams, CancellationToken ct = default)
    {
        // Sayfalama ve filtre parametrelerine göre benzersiz bir anahtar oluşturuyoruz
        string cacheKey = $"customers:list:l{custParams.LastId}:s{custParams.PageSize}:{custParams.Search}";

        var customerDtos = await _cache.GetOrCreateAsync(
            cacheKey,
            async token => 
            {
                var spec = new CustomerCursorSpecification(custParams);
                var customers = await _repository.ListAsync(spec, token);

                return customers.Select(c => new CustomerReadDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Balance = c.Balance,
                    CreatedAt = c.CreatedAt,
                    TcKimlikNo = c.TcKimlikNo,
                    ProfileImageId = c.ProfileImageId,
                    BirthDate = c.BirthDate
                }).ToList();
            },
            tags: new[] { "customers_list" }, // Tag kullanımı toplu silmeyi kolaylaştırır
            cancellationToken: ct
        );

        return new SuccessDataResult<List<CustomerReadDto>>(customerDtos, "Customers retrieved");
    }

    public async Task<IDataResult<CustomerReadDto>> GetCustomerById(int id, CancellationToken ct = default)
    {
        string cacheKey = $"customer:{id}";

        var data = await _cache.GetOrCreateAsync(
            cacheKey,
            async token => 
            {
                var customer = await _repository.GetByIdAsync(id, token);
                if (customer == null) return null;

                return new CustomerReadDto { 
                    Id = customer.Id, 
                    Name = customer.Name, 
                    Email = customer.Email, 
                    Balance = customer.Balance,
                    CreatedAt = customer.CreatedAt,
                    TcKimlikNo = customer.TcKimlikNo,
                    ProfileImageId = customer.ProfileImageId,
                    BirthDate = customer.BirthDate,
                    ProfileImagePath = customer.ProfileImage != null 
                        ? _filesService.GetPublicUrl(customer.ProfileImage) 
                        : null
                };
            },
            cancellationToken: ct
        );

        if (data == null) 
            return new ErrorDataResult<CustomerReadDto>(_localizer["CustomerNotFound"]);

        return new SuccessDataResult<CustomerReadDto>(data);
    }

    public async Task<IResult> UpdateCustomer(CustomerUpdateDto updateDto, CancellationToken ct = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(updateDto, ct);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return new ErrorResult(errors);
        }

        var existingCustomer = await _repository.GetByIdAsync(updateDto.Id, ct);
        if (existingCustomer == null) 
            return new ErrorResult(_localizer["CustomerNotFound"]);
        
        // Güncelleme mantığı...
        existingCustomer.Name = updateDto.Name;
        existingCustomer.Email = updateDto.Email;
        existingCustomer.Balance = updateDto.Balance;
        existingCustomer.TcKimlikNo = updateDto.TcKimlikNo;
        existingCustomer.BirthDate = updateDto.BirthDate.HasValue 
            ? DateTime.SpecifyKind(updateDto.BirthDate.Value, DateTimeKind.Utc) 
            : null;

        await _repository.UpdateAsync(existingCustomer, ct);

        // --- CACHE INVALIDATION ---
        await _cache.RemoveAsync($"customer:{updateDto.Id}", ct);
        await _cache.RemoveByTagAsync("customers_list", ct);        // Listeleri temizle

        // SignalR...
        var userName = _currentUserService.UserName ?? "Sistem";
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", new {
            User = userName,
            Action = "CustomerUpdated",
            Message = _localizer["CustomerUpdatedNotification", userName, updateDto.Name].Value
        }, ct);

        return new SuccessResult(_localizer["CustomerUpdated"]);
    }
public async Task<IResult> DeleteCustomer(CustomerDeleteDto deleteDto, CancellationToken ct = default)
{
    var validationResult = await _deleteValidator.ValidateAsync(deleteDto, ct);
    if (!validationResult.IsValid)
    {
        var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
        return new ErrorResult(errors);
    }

    var existingCustomer = await _repository.GetByIdAsync(deleteDto.Id, ct);
    var customerName = existingCustomer?.Name ?? $"ID:{deleteDto.Id}";

    var deleted = await _repository.DeleteAsync(deleteDto.Id, ct);
    if (!deleted) return new ErrorResult(_localizer["DeleteError"]);

    // --- CACHE INVALIDATION ---
    // Burada hata aldığın yer: updateDto.Id yerine deleteDto.Id kullanıyoruz
    await _cache.RemoveAsync($"customer:{deleteDto.Id}", ct);
    
    // Listeleri temizlemek için etiketi (tag) siliyoruz
    await _cache.RemoveByTagAsync("customers_list", ct);

    // --- SIGNALR BİLDİRİMİ ---
    var userName = _currentUserService.UserName ?? "Sistem";
    await _hubContext.Clients.All.SendAsync("ReceiveNotification", new {
        User = userName,
        Action = "CustomerDeleted",
        Message = _localizer["CustomerDeletedNotification", userName, customerName].Value
    }, ct);

    return new SuccessResult(_localizer["CustomerDeleted"]);
}
}