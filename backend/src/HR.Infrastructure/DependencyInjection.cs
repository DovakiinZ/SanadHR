using Amazon.S3;
using HR.Application.Common.Interfaces;
using HR.Infrastructure.Persistence;
using HR.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace HR.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // PostgreSQL + EF Core
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Dapper
        services.AddSingleton(new DapperContext(configuration.GetConnectionString("DefaultConnection")!));

        // Redis
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            // Connect lazily on first use — avoids failing host startup (and EF design-time tooling)
            // when Redis isn't running.
            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
            services.AddScoped<ICacheService, RedisCacheService>();
        }

        // Cloudflare R2 (S3-compatible)
        var r2Config = configuration.GetSection("R2");
        if (r2Config.Exists())
        {
            services.AddSingleton<IAmazonS3>(sp =>
            {
                var config = new AmazonS3Config
                {
                    ServiceURL = r2Config["ServiceUrl"],
                    ForcePathStyle = true
                };
                return new AmazonS3Client(r2Config["AccessKey"], r2Config["SecretKey"], config);
            });
            services.AddScoped<IFileStorageService>(sp =>
                new R2FileStorageService(sp.GetRequiredService<IAmazonS3>(), r2Config["BucketName"]!));
        }

        // Audit
        services.AddScoped<IAuditLogService, AuditLogService>();

        // Financial Calculation Engine
        services.AddScoped<HR.Application.Engines.Finance.IFinancialLedger, HR.Infrastructure.Engines.Finance.FinancialLedger>();
        services.AddScoped<HR.Application.Engines.Finance.IRuleEngine, HR.Infrastructure.Engines.Finance.RuleEngine>();
        services.AddScoped<HR.Application.Engines.Finance.IPayrollFactProvider, HR.Infrastructure.Engines.Finance.PayrollFactProvider>();
        services.AddScoped<HR.Infrastructure.Engines.Finance.PayrollComputation>();
        services.AddScoped<HR.Application.Engines.Finance.IPayrollPreviewEngine, HR.Infrastructure.Engines.Finance.PayrollPreviewEngine>();
        services.AddScoped<HR.Application.Engines.Finance.IPayrollRunEngine, HR.Infrastructure.Engines.Finance.PayrollRunEngine>();

        // Payroll validation engine + validators (specification pattern; add a class to add a check)
        services.AddScoped<HR.Application.Engines.Finance.IPayrollValidationEngine, HR.Infrastructure.Engines.Finance.PayrollValidationEngine>();
        services.AddScoped<HR.Application.Engines.Finance.IPayrollValidator, HR.Infrastructure.Engines.Finance.Validators.NegativeSalaryValidator>();
        services.AddScoped<HR.Application.Engines.Finance.IPayrollValidator, HR.Infrastructure.Engines.Finance.Validators.InvalidGosiValidator>();
        services.AddScoped<HR.Application.Engines.Finance.IPayrollValidator, HR.Infrastructure.Engines.Finance.Validators.DuplicateEmployeeValidator>();
        services.AddScoped<HR.Application.Engines.Finance.IPayrollValidator, HR.Infrastructure.Engines.Finance.Validators.OverlappingPayrollValidator>();
        services.AddScoped<HR.Application.Engines.Finance.IPayrollValidator, HR.Infrastructure.Engines.Finance.Validators.CurrencyValidator>();
        services.AddScoped<HR.Application.Engines.Finance.IPayrollValidator, HR.Infrastructure.Engines.Finance.Validators.MissingAttendanceValidator>();
        services.AddScoped<HR.Application.Engines.Finance.IPayrollValidator, HR.Infrastructure.Engines.Finance.Validators.RuleConflictValidator>();

        return services;
    }
}
