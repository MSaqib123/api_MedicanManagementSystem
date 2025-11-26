using Amazon.S3;
using AspNetCoreRateLimit;
using FluentValidation.AspNetCore;
using MedicineManagementSystem.BackgroundServices;
using MedicineManagementSystem.Data;
using MedicineManagementSystem.Middlewares;
using MedicineManagementSystem.Models;
using MedicineManagementSystem.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Logging with Serilog
//dotnet add package Serilog
//dotnet add package Serilog.Sinks.Console
//dotnet add package Serilog.Sinks.File
//dotnet add package Serilog.AspNetCore
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers().AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Program>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddSwaggerGen(options =>
{
    var provider = builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerDoc(description.GroupName, new OpenApiInfo
        {
            Title = $"Medicine Management API {description.ApiVersion}",
            Version = description.ApiVersion.ToString()
        });
    }
});

// Database Context - Multi-tenant with Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Setup
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Can enable for advanced
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Authentication - JWT integrated with Identity
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin").RequireClaim("TenantAccess", "Full"));
    options.AddPolicy("Pharmacist", policy => policy.RequireRole("Pharmacist").RequireClaim("BranchAccess", "ReadWrite"));
    options.AddPolicy("Accountant", policy => policy.RequireRole("Accountant").RequireClaim("FinancialAccess", "View"));
    // More policies
});

// Caching with Redis
// Microsoft.Extensions.Caching.StackExchangeRedis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// Data Protection for encryption
builder.Services.AddDataProtection();

// Custom Middleware
//builder.Services.AddScoped<TenantMiddleware>();
//builder.Services.AddScoped<ErrorHandlingMiddleware>();

// Services Injection
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IMedicineService, MedicineService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IIntegrationService, IntegrationService>();
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddAWSService<IAmazonS3>(); // registers IAmazonS3

// Background Services for alerts
builder.Services.AddHostedService<ExpiryAlertBackgroundService>();
builder.Services.AddHostedService<LowStockAlertBackgroundService>();
builder.Services.AddHostedService<DuePaymentAlertBackgroundService>();


// CORS for multi-platform
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});


builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseIpRateLimiting();
app.UseAuthentication();
app.UseAuthorization();
//app.UseMiddleware<TenantMiddleware>();
app.MapControllers();

app.Run();

















//using Amazon.S3;
//using AspNetCoreRateLimit;
//using FluentValidation.AspNetCore;
//using MedicineManagementSystem.BackgroundServices;
//using MedicineManagementSystem.Data;
//using MedicineManagementSystem.Middlewares;
//using MedicineManagementSystem.Models;
//using MedicineManagementSystem.Services;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.ApiExplorer;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi;
//using Serilog;
//using StackExchange.Redis;
//using System.Reflection;
//using System.Text;

//var builder = WebApplication.CreateBuilder(args);

//-------------------------------------------------
//1.Serilog
//------------------------------------------------ -
//Log.Logger = new LoggerConfiguration()
//   .WriteTo.Console()
//   .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
//   .CreateLogger();
//builder.Host.UseSerilog();

//-------------------------------------------------
//2.Controllers + FluentValidation
//------------------------------------------------ -
//builder.Services.AddControllers()
//   .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Program>());

//-------------------------------------------------
//3.API Versioning
//------------------------------------------------ -
//builder.Services.AddApiVersioning(opt =>
//{
//   opt.DefaultApiVersion = new ApiVersion(1, 0);
//   opt.AssumeDefaultVersionWhenUnspecified = true;
//   opt.ReportApiVersions = true;
//});

//builder.Services.AddVersionedApiExplorer(opt =>
//{
//    opt.GroupNameFormat = "'v'VVV";
//    opt.SubstituteApiVersionInUrl = true;
//});

//-------------------------------------------------
//4.Swagger(Swashbuckle) – .NET 10 compatible
// -------------------------------------------------
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo
//                     {
//                         Title = "Medicine Management API",
//                         Version = "v1"
//                     });

//Enable file upload support
//    c.OperationFilter<FileUploadOperationFilter>();

//JWT bearer definition (unchanged)
//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//                                    {
//                                        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer {token}')",
//                                        Name = "Authorization",
//                                        In = ParameterLocation.Header,
//                                        Type = SecuritySchemeType.Http,
//                                        Scheme = "bearer",
//                                        BearerFormat = "JWT"
//                                    });
//});

//-------------------------------------------------
//5.Database + Identity
//------------------------------------------------ -
//builder.Services.AddDbContext<ApplicationDbContext>(opt =>
//   opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(opt =>
//{
//    opt.Password.RequireDigit = true;
//    opt.Password.RequiredLength = 8;
//    opt.Password.RequireNonAlphanumeric = true;
//    opt.Password.RequireUppercase = true;
//    opt.Password.RequireLowercase = true;
//    opt.Lockout.MaxFailedAccessAttempts = 5;
//    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
//    opt.User.RequireUniqueEmail = true;
//})
//.AddEntityFrameworkStores<ApplicationDbContext>()
//.AddDefaultTokenProviders();

//-------------------------------------------------
//6.JWT Authentication
//------------------------------------------------ -
//var jwtKey = builder.Configuration["Jwt:Key"]
//            ?? throw new InvalidOperationException("Jwt:Key missing in configuration");
//var keyBytes = Encoding.ASCII.GetBytes(jwtKey);

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(opt =>
//    {
//        opt.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = builder.Configuration["Jwt:Issuer"],
//            ValidAudience = builder.Configuration["Jwt:Audience"],
//            IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
//        };
//    });

//builder.Services.AddAuthorization(opt =>
//{
//    opt.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
//    opt.AddPolicy("Pharmacist", p => p.RequireRole("Pharmacist"));
//    opt.AddPolicy("Accountant", p => p.RequireRole("Accountant"));
//});

//-------------------------------------------------
//7.Redis cache
//------------------------------------------------ -
//builder.Services.AddStackExchangeRedisCache(opt =>
//{
//   opt.Configuration = builder.Configuration.GetConnectionString("Redis");
//});

//-------------------------------------------------
//8.Rate limiting(AspNetCoreRateLimit)
//------------------------------------------------ -
//builder.Services.AddMemoryCache();
//builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
//builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
//builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
//builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
//builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

//-------------------------------------------------
//9.Data protection, S3, DI services, background jobs
// -------------------------------------------------
//builder.Services.AddDataProtection();
//builder.Services.AddAWSService<IAmazonS3>();

//builder.Services.AddScoped<ITenantService, TenantService>();
//builder.Services.AddScoped<IBranchService, BranchService>();
//builder.Services.AddScoped<IMedicineService, MedicineService>();
//builder.Services.AddScoped<IInventoryService, InventoryService>();
//builder.Services.AddScoped<ISalesService, SalesService>();
//builder.Services.AddScoped<IPurchaseService, PurchaseService>();
//builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
//builder.Services.AddScoped<IUserService, UserService>();
//builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
//builder.Services.AddScoped<INotificationService, NotificationService>();
//builder.Services.AddScoped<IIntegrationService, IntegrationService>();
//builder.Services.AddScoped<IBackupService, BackupService>();

//builder.Services.AddHostedService<ExpiryAlertBackgroundService>();
//builder.Services.AddHostedService<LowStockAlertBackgroundService>();
//builder.Services.AddHostedService<DuePaymentAlertBackgroundService>();

//-------------------------------------------------
//10.CORS
//------------------------------------------------ -
//builder.Services.AddCors(opt =>
//   opt.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

//-------------------------------------------------
//BUILD APP
//------------------------------------------------ -
//var app = builder.Build();

//-------------------------------------------------
//11.Swagger UI – default URL = /swagger
// -------------------------------------------------
//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();

//app.UseSwagger();
//app.UseSwaggerUI(c =>
//{
//    one endpoint per version(v1 now, you can add more later)

//    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Medicine Management API v1");
//    c.RoutePrefix = "swagger";               // <-- open https://localhost:<port>/swagger
//    c.DisplayOperationId();                  // nice for client generation
//    c.DisplayRequestDuration();
//});
//}
//else
//{
//    app.UseExceptionHandler("/error");
//    app.UseHsts();
//}

//-------------------------------------------------
//12.Pipeline
//------------------------------------------------ -
//app.UseHttpsRedirection();
//app.UseCors("AllowAll");
//app.UseMiddleware<ErrorHandlingMiddleware>();
//app.UseIpRateLimiting();
//app.UseAuthentication();
//app.UseAuthorization();
//app.UseMiddleware<TenantMiddleware>();

//app.MapControllers();

//app.Run();



////var builder = WebApplication.CreateBuilder(args);

////// Add services to the container.

////builder.Services.AddControllers();
////// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
////builder.Services.AddOpenApi();

////var app = builder.Build();

////// Configure the HTTP request pipeline.
////if (app.Environment.IsDevelopment())
////{
////    app.MapOpenApi();
////}
////app.UseHttpsRedirection();
////app.UseAuthorization();
////app.MapControllers();
////app.Run();

