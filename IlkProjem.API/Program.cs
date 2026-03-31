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
using Supabase; // 1. ADIM: Supabase kütüphanesini ekle
using IlkProjem.Core.Constants;
using IlkProjem.Core.Constants;

string logo = @"
 '||''|.                    '||          |                        
  ||   ||   ....   .. ...    ||  ..     |||    ... ...   ... ...  
  ||'''|.  '' .||   ||  ||   || .'     |  ||    ||'  ||   ||'  || 
  ||    || .|' ||   ||  ||   ||'|.    .''''|.   ||    |   ||    | 
 .||...|'  '|..'|' .||. ||. .||. ||. .|.  .||.  ||...'    ||...'  
                                                ||        ||      
                                               ''''      ''''     ";

Console.WriteLine(logo);

var builder = WebApplication.CreateBuilder(args);

// --- 0. BAĞLANTI CÜMLESİ TAMİRCİSİ ---
var rawConnection = builder.Configuration.GetConnectionString("DefaultConnection") 
                    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                    ?? Environment.GetEnvironmentVariable("DATABASE_URL")
                    ?? Environment.GetEnvironmentVariable("DATABASE_PRIVATE_URL");

string connectionString = rawConnection ?? "";

if (!string.IsNullOrEmpty(rawConnection) && rawConnection.StartsWith("postgres"))
{
    var databaseUri = new Uri(rawConnection);
    var userInfo = databaseUri.UserInfo.Split(':');
    connectionString = $"Host={databaseUri.Host};Port={databaseUri.Port};Database={databaseUri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Prefer;Trust Server Certificate=true;";
}

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

// --- 3. DATABASE & SUPABASE SETUP ---
builder.Services.AddScoped<AuditSaveChangesInterceptor>();
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
    options.UseNpgsql(connectionString)
           .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));

// 2. ADIM: Supabase Client Kaydı
// appsettings.json içindeki "Supabase:Url" ve "Supabase:Key" değerlerini okur
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:Key"];

builder.Services.AddSingleton(provider => 
    new Supabase.Client(supabaseUrl, supabaseKey, new SupabaseOptions { AutoConnectRealtime = true }));

// --- 4. AUTHENTICATION & JWT ---
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
    x.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notification-hub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// --- 4b. AUTHORIZATION POLICIES (PBAC GÜNCELLEMESİ) ---
builder.Services.AddAuthorization(options =>
{
    // Admin + Manager rollerinin yapabildiği işlemleri spesifik yetkilere bağlıyoruz.
    // RequireClaim("permissions", ...) kullanımı: Kullanıcının "permissions" claim'i olmalı 
    // ve değeri bu dizideki yetkilerden EN AZ BİRİYLE eşleşmeli.
    
    options.AddPolicy(Policies.CustomerManagement, policy =>
        policy.RequireClaim("permissions", 
            Permissions.Customers.View, 
            Permissions.Customers.Create, 
            Permissions.Customers.Edit, 
            Permissions.Customers.Delete));

    // Sadece Admin'in yapabildiği, sistemsel yetkiler
    options.AddPolicy(Policies.AdminOnly, policy =>
        policy.RequireClaim("permissions", Permissions.System.Manage));

    // Kullanıcı yönetimi yetkileri
    options.AddPolicy(Policies.UserManagement, policy =>
        policy.RequireClaim("permissions", 
            Permissions.Users.View, 
            Permissions.Users.Create, 
            Permissions.Users.Edit, 
            Permissions.Users.Delete));

    // Dosya/Asset yönetimi yetkileri
    options.AddPolicy(Policies.FileManagement, policy =>
        policy.RequireClaim("permissions", 
            Permissions.Files.Upload, 
            Permissions.Files.Delete, 
            Permissions.Files.View));
});
// --- 5. RATE LIMITING ---
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
                PermitLimit = 1000,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
    
    options.RejectionStatusCode = 429;
});

// --- 5b. SIGNALR SETUP ---
builder.Services.AddSignalR();


builder.Services.AddHttpContextAccessor();

// DI Registrations
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ICalculatorService, CalculatorService>();
builder.Services.AddScoped<IExcelService, ExcelService>();
builder.Services.AddScoped<IFilesService, FilesService>(); // Bu artık Supabase kullanacak
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

// --- 6. CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy.WithOrigins("http://localhost:4200", "https://bank-app-ui-v2.vercel.app") 
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
});

// --- 7. SERILOG ---
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 31,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.PostgreSQL(connectionString, "ServiceLog") 
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// --- 8. MIDDLEWARE ---
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);
app.UseMiddleware<ExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// NOT: Artık Supabase kullandığımız için "app.UseStaticFiles" (yerel uploads klasörü) 
// devre dışı bırakıldı. Dosyalar artık Supabase URL'leri üzerinden çekilecek.

app.UseRouting();
app.UseCors("AllowAngular");
app.UseMiddleware<LoggingMiddleware>();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapHub<IlkProjem.Core.Hubs.NotificationHub>("/notification-hub");
app.MapControllers().RequireRateLimiting("PerUser");

// --- 9. DATABASE MIGRATION & SEED DATA ---
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        context.Database.Migrate();
    }

    if (!context.Customers.Any())
    {
        context.Customers.AddRange(CustomerSeeder.GetFakeCustomers(50));
        context.SaveChanges();
    }
}

app.MapOpenApi();

// --- 10. APP RUN ---
if (app.Environment.IsDevelopment())
{
    app.Run();
}
else
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5005";
    app.Run($"http://0.0.0.0:{port}");
}