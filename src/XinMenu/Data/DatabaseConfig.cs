using Daibitx.EFCore.AutoMigrate.Extension;
using Daibitx.Identity.EFCore;
using Daibitx.Identity.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite.Design.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal;
using XinMenu.Entitys;

namespace XinMenu.Data;

public static class DatabaseConfig
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseProvider = configuration.GetValue<string>("DatabaseProvider", "Sqlite");
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=XinMenu.db";

        services.AddDbContext<AppDbContext>(options =>
        {
            switch (databaseProvider?.ToLowerInvariant())
            {
                case "postgresql":
                case "postgres":
                    options.UseNpgsql(connectionString);
                    break;

                case "sqlite":
                default:
                    options.UseSqlite(connectionString);
                    break;
            }

            if (configuration.GetValue<bool>("EnableSensitiveDataLogging", false))
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        return services;
    }

    public static void AddAuthorization(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddDaibitxIdentity(identityBuilder =>
        {
            identityBuilder.AddIdentityEFCore<User, Role>(options =>
            {
                var databaseProvider = configuration.GetValue<string>("DatabaseProvider", "Sqlite");
                var connectionString = configuration.GetConnectionString("DefaultConnection")
                    ?? "Data Source=XinMenu.db";

                switch (databaseProvider?.ToLowerInvariant())
                {
                    case "postgresql":
                    case "postgres":
                        options.UseNpgsql(connectionString);
                        break;

                    case "sqlite":
                    default:
                        options.UseSqlite(connectionString);
                        break;
                }
            }).Build();
            if (builder.Environment.IsDevelopment())
            {
                identityBuilder.ConfigureApiScopes<User, Role>(options =>
                {
                    options.EnableOverrideExistingScopes = true;
                    options.AutoDiscovery = true;
                });
            }
            else
            {
                identityBuilder.ConfigureApiScopes<User, Role>(options =>
                {
                    options.EnableOverrideExistingScopes = false;
                    options.AutoDiscovery = true;
                });
            }
            identityBuilder.UseMemoryCache<User, Role>();
            identityBuilder.UseBcryptPasswordHasher();
            identityBuilder.UseJwtAuthentication(builder.Configuration);
            identityBuilder.AddIdentitySession();
            identityBuilder.AddAuthorizationServices();
        });
    }

    public static void InitializeDatabase(this WebApplication app)
    {
        app.Services.AutoMigrate<AppDbContext>(services =>
        {
            new SqliteDesignTimeServices().ConfigureDesignTimeServices(services);
            new NpgsqlDesignTimeServices().ConfigureDesignTimeServices(services);
        }, options =>
        {
            options.AsFullMode();
        });
    }


}
