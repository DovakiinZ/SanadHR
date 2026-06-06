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

        return services;
    }
}
