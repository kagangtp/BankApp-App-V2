using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using IlkProjem.DAL.Data;
using IlkProjem.BLL.Services;
using IlkProjem.DAL.Repositories;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using System.Text;
using System.Threading.RateLimiting;
using IlkProjem.Core.Interfaces;
using IlkProjem.DAL.Interceptors;
using IlkProjem.API.Middlewares;
using Serilog;
using FluentValidation;
using IlkProjem.BLL.Interfaces;
using IlkProjem.DAL.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// --- 1. LOCALIZATION SETUP ---
builder.Services.AddLocalization();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var cultures = new[] { "en-US", "tr-TR" };
    options.DefaultRequestCulture = new RequestCulture("tr-TR");
    options.SupportedCultures = cultures.Select(c => new CultureInfo(c)).ToList();
    options.SupportedUICultures = cultures.Select(c => new CultureInfo(c)).ToList();
    options.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());
});

// --- 2. OPENAPI / SWAGGER SETUP ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        var localizer = context.ApplicationServices.GetRequiredService<IStringLocalizer<Program>>();
        document.Info.Title = localizer["ApiTitle"];
        return Task.CompletedTask;
    });
});

// --- 3. DATABASE & SERVICES ---
builder.Services.AddScoped<AuditSaveChangesInterceptor>();
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));

var keyString = builder.Configuration["Jwt:Key"] ?? "fallback_secret_key_32_characters_long";
var key = Encoding.ASCII.GetBytes(keyString);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true
    };
});

// --- 4. RATE LIMITING ---
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter("Global", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100, 
            Window = TimeSpan.FromSeconds(1),
            QueueLimit = 0
        }));

   options.AddPolicy("PerUser", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
    
    options.RejectionStatusCode = 429;
});

builder.Services.AddHttpContextAccessor();

// DI Registrations
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ICalculatorService, CalculatorService>();
builder.Services.AddScoped<IExcelService, ExcelService>();
builder.Services.AddScoped<IFilesService, FilesService>();
builder.Services.AddScoped<IFilesRepository, FilesRepository>();
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IHouseRepository, HouseRepository>();
builder.Services.AddScoped<IHouseService, HouseService>();
builder.Services.AddScoped<IMailService, MailManager>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddValidatorsFromAssemblyContaining<IlkProjem.BLL.ValidationRules.FluentValidation.CustomerDtoValidators.CustomerCreateDtoValidator>();

builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// --- 5. CORS (Vercel URL'ini buraya eklemeyi unutma!) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy.WithOrigins("http://localhost:4200", "https://your-vercel-app.vercel.app") 
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
});

// --- 6. SERILOG SETUP ---
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.PostgreSQL(builder.Configuration.GetConnectionString("DefaultConnection"), "ServiceLog")
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// --- 7. MIDDLEWARE ---
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- 8. STATIC FILE SERVING (GÜVENLİ YOL OLUŞTURMA) ---
// Sadece null değil, boş string gelme ihtimaline karşı kontrol eklendi
var configPath = builder.Configuration["FileSettings:StoragePath"];
var storagePath = !string.IsNullOrWhiteSpace(configPath) 
    ? configPath 
    : Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

if (!Directory.Exists(storagePath))
    Directory.CreateDirectory(storagePath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(storagePath),
    RequestPath = "/uploads"
});

app.UseRouting();
app.UseCors("AllowAngular");
app.UseMiddleware<LoggingMiddleware>();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.MapControllers().RequireRateLimiting("PerUser");

// --- 9. DATABASE MIGRATION & SEED DATA ---
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Railway'de tabloların otomatik oluşması için Migrate() eklendi
    context.Database.Migrate();

    if (!context.Customers.Any())
    {
        context.Customers.AddRange(CustomerSeeder.GetFakeCustomers(50));
        context.SaveChanges();
    }
}

app.MapOpenApi();

// --- 10. RAILWAY PORT AYARI ---
// Sabit localhost yerine Railway'in verdiği PORT değişkenini okur
var port = Environment.GetEnvironmentVariable("PORT") ?? "5005";
app.Run($"http://0.0.0.0:{port}");