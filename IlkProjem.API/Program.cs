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
using IlkProjem.Core.Constants;
using Quartz;
using IlkProjem.BLL.Jobs;
using IlkProjem.API.Helpers;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;

BannerHelper.PrintLogo();

// --- 0. .ENV LOAD ---
// Traverse path to find the frontend/backend adjacent outer .env file
DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// --- 0. BAĞLANTI CÜMLESİ TAMİRCİSİ (POSTGRES) ---
var configDb = builder.Configuration.GetConnectionString("DefaultConnection");
var envDb = Environment.GetEnvironmentVariable("DATABASE_URL") 
            ?? Environment.GetEnvironmentVariable("DATABASE_PRIVATE_URL");

// Boş tırnak veya null kontrolü yaparak gerçek bağlantıyı seçiyoruz
string rawConnection = !string.IsNullOrWhiteSpace(configDb) ? configDb : (envDb ?? "");

string connectionString = rawConnection;

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

// --- 3. DATABASE SETUP ---
builder.Services.AddScoped<AuditSaveChangesInterceptor>();
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
    options.UseNpgsql(connectionString)
           .AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>()));

// --- 3b. REDIS SETUP (GÜVENLİ VE AKILLI) ---
var envRedisUrl = Environment.GetEnvironmentVariable("REDIS_URL");
var configRedisUrl = builder.Configuration.GetConnectionString("RedisConnection");

string redisConfig = string.Empty;

if (!string.IsNullOrWhiteSpace(envRedisUrl))
{
    try 
    {
        var uri = new Uri(envRedisUrl);
        var password = uri.UserInfo.Contains(':') ? uri.UserInfo.Split(':').Last() : uri.UserInfo;
        // StackExchange.Redis için format: host:port,password=xxx
        redisConfig = $"{uri.Host}:{uri.Port},password={password},abortConnect=false,ssl=false";
        Console.WriteLine($"[REDIS SUCCESS]: Render REDIS_URL aktif.");
    }
    catch 
    {
        redisConfig = envRedisUrl; // Parse edemezse düz kullanmayı dene
    }
}
else if (!string.IsNullOrWhiteSpace(configRedisUrl))
{
    redisConfig = configRedisUrl;
}

if (string.IsNullOrWhiteSpace(redisConfig))
{
    Console.WriteLine("[REDIS WARNING]: Redis connection string is missing from env or config. Falling back to local network default.");
    redisConfig = "localhost:6379"; // Last resort safeguard for local dev if not defined in .env
}


Console.WriteLine($"[REDIS CONFIG]: {redisConfig.Split(',')[0]} (Password hidden)");

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConfig;
    options.InstanceName = "IlkProjem_";
});

// Hybrid Cache (.NET 9/10 özelliği)
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromHours(1),
        LocalCacheExpiration = TimeSpan.FromMinutes(5)
    };
});

// --- 4. AUTHENTICATION & JWT ---
var keyString = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(keyString))
{
    throw new InvalidOperationException("JWT Key is missing from configuration. Add it to .env or appsettings.json.");
}
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

// --- 4b. AUTHORIZATION POLICIES ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.CustomerManagement, policy =>
        policy.RequireClaim("permissions", Permissions.Customers.View, Permissions.Customers.Create, Permissions.Customers.Edit, Permissions.Customers.Delete));
    options.AddPolicy(Policies.AdminOnly, policy =>
        policy.RequireClaim("permissions", Permissions.System.Manage));
    options.AddPolicy(Policies.UserManagement, policy =>
        policy.RequireClaim("permissions", Permissions.Users.View, Permissions.Users.Create, Permissions.Users.Edit, Permissions.Users.Delete));
    options.AddPolicy(Policies.FileManagement, policy =>
        policy.RequireClaim("permissions", Permissions.Files.Upload, Permissions.Files.Delete, Permissions.Files.View));
});

// --- 5. RATE LIMITING & SIGNALR ---
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

builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// DI Registrations
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
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
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IAiChatMessageRepository, AiChatMessageRepository>();
// AiChatService: HttpClient + ICustomerService + ICarService + IHouseService + IKnowledgeService inject alır.
builder.Services.AddHttpClient<IAiChatService, AiChatService>();

// --- RAG SERVİSLERİ ---
builder.Services.AddScoped<IKnowledgeRepository, KnowledgeRepository>();
builder.Services.AddHttpClient<IEmbeddingService, EmbeddingService>();
builder.Services.AddScoped<IKnowledgeService, KnowledgeService>();

// --- WORKFLOW SERVİSLERİ ---
builder.Services.AddScoped<IWorkflowRepository, WorkflowRepository>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IBusinessActionService, BusinessActionService>();

// --- 5b. GCP STORAGE CLIENT (Singleton) ---
builder.Services.AddSingleton<StorageClient>(sp =>
{
    var gcpCredJson = Environment.GetEnvironmentVariable("GCP_CREDENTIALS_JSON");
    if (!string.IsNullOrWhiteSpace(gcpCredJson))
    {
        // Production (Render): Inline JSON credential
        var credential = GoogleCredential.FromJson(gcpCredJson);
        Console.WriteLine("[GCP SUCCESS]: Service Account credential loaded from GCP_CREDENTIALS_JSON.");
        return StorageClient.Create(credential);
    }

    // Local dev: Application Default Credentials (gcloud auth application-default login)
    Console.WriteLine("[GCP INFO]: Using Application Default Credentials (local dev).");
    return StorageClient.Create();
});

// --- 5c. QUARTZ SETUP ---
builder.Services.AddQuartz(quartz =>
{
    var jobKey = new JobKey("DailyMailJob");
    quartz.AddJob<DailyMailJob>(opts => opts.WithIdentity(jobKey));
    quartz.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("DailyMailJob-Trigger")
        .WithCronSchedule("0 0 9 * * ?", x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"))));
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddValidatorsFromAssemblyContaining<IlkProjem.BLL.ValidationRules.FluentValidation.CustomerDtoValidators.CustomerCreateDtoValidator>();

builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// --- 6. CORS ---
var corsOrigins = Environment.GetEnvironmentVariable("ASPNETCORE_CORS_ORIGINS")?.Split(',') ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy.WithOrigins(corsOrigins) 
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
});

// --- 7. SERILOG ---
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.PostgreSQL(connectionString, "ServiceLog", needAutoCreateTable: true) 
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
        try {
            context.Database.Migrate();
            if (!context.Customers.Any())
            {
                context.Customers.AddRange(CustomerSeeder.GetFakeCustomers(50));
                context.SaveChanges();
            }

            // RAG Bilgi Tabanı Seed işlemi kaldırıldı (Mock data iptal edildi)
        }
        catch (Exception ex) {
            Console.WriteLine($"[MIGRATION ERROR]: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"[MIGRATION INNER ERROR]: {ex.InnerException.Message}");
            }
        }
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
    // Render için dinamik port kullanımı
    var port = Environment.GetEnvironmentVariable("PORT");
    if (!string.IsNullOrEmpty(port))
    {
        app.Run($"http://0.0.0.0:{port}");
    }
    else
    {
        // Kullanıcı .env file üzerinde ASPNETCORE_URLS verebilir.
        app.Run();
    }
}