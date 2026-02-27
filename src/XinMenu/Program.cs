using Serilog;
using Serilog.Events;
using XinMenu.Data;
using XinMenu.Services.Abstractions;
using XinMenu.Services.Inplementations;

namespace XinMenu;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "XinMenu API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
        });


        builder.Host.UseSerilog((ctx, lc) =>
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string logsDirectory = Path.Combine(Directory.GetParent(baseDirectory).FullName, "..", "log");
            Directory.CreateDirectory(logsDirectory);

            string dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
            string datePath = Path.Combine(logsDirectory, dateFolder);
            Directory.CreateDirectory(datePath);

            lc.MinimumLevel.Information()
              .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
              .MinimumLevel.Override("System", LogEventLevel.Warning)
              .Enrich.FromLogContext()
              .Enrich.WithProperty("Application", "Celora")
              .WriteTo.Logger(sub => sub
                  .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
                  .WriteTo.Async(a => a.File(
                      Path.Combine(datePath, "Information-.log"),
                      rollingInterval: RollingInterval.Day,
                      outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")))
              .WriteTo.Logger(sub => sub
                  .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
                  .WriteTo.Async(a => a.File(
                      Path.Combine(datePath, "Warning-.log"),
                      rollingInterval: RollingInterval.Day,
                      outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")))
              .WriteTo.Logger(sub => sub
                  .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error)
                  .WriteTo.Async(a => a.File(
                      Path.Combine(datePath, "Error-.log"),
                      rollingInterval: RollingInterval.Day,
                      outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")))
              .WriteTo.Logger(sub => sub
                  .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Fatal)
                  .WriteTo.Async(a => a.File(
                      Path.Combine(datePath, "Fatal-.log"),
                      rollingInterval: RollingInterval.Day,
                      outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")));
        });

        builder.AddAuthorization(builder.Configuration);

        builder.Services.AddDatabase(builder.Configuration);

        builder.Services.AddHttpContextAccessor();

        // Register Services
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IRecipeService, RecipeService>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<IIngredientService, IngredientService>();
        builder.Services.AddScoped<IFoodLogService, FoodLogService>();
        builder.Services.AddScoped<IUserService, UserService>();


        var app = builder.Build();

        app.InitializeDatabase();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // Enable static files
        app.UseStaticFiles();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();

    }
}
