using HenryTires.Inventory.Application.Extensions;
using HenryTires.Inventory.Application.Ports;
using HenryTires.Inventory.Domain.Services;
using HenryTires.Inventory.Infrastructure.Data;
using HenryTires.Inventory.Infrastructure.Repositories;
using HenryTires.Inventory.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace HenryTires.Inventory.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString =
            Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING")
            ?? configuration.GetConnectionString("MongoDbLocal");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("MongoDB connection string not configured");
        }

        // Configure MongoDB client with SSL settings to fix Linux/Railway SSL issues
        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.SslSettings = new MongoDB.Driver.SslSettings
        {
            EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12,
            CheckCertificateRevocation = false
        };
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(30);

        services.AddSingleton<IMongoClient>(sp => new MongoClient(settings));

        var databaseName = configuration["MongoDb:DatabaseName"] ?? "Inventory";
        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return new MongoDbContext(client, databaseName);
        });

        // Repositories - Register both interface and implementation
        services.AddSingleton<IBranchRepository, BranchRepository>();
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IItemRepository, ItemRepository>();
        services.AddSingleton<IConsumableItemPriceRepository, ConsumableItemPriceRepository>();
        services.AddSingleton<IInventorySummaryRepository, InventorySummaryRepository>();
        services.AddSingleton<IInventoryTransactionRepository, InventoryTransactionRepository>();
        services.AddSingleton<ISaleRepository, SaleRepository>();

        // Services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<ICurrentUser, CurrentUserService>();

        // Transaction and identity adapters
        services.AddSingleton<HenryTires.Inventory.Application.Ports.Outbound.IUnitOfWork, HenryTires.Inventory.Infrastructure.Adapters.Transactions.MongoUnitOfWork>();
        services.AddSingleton<HenryTires.Inventory.Application.Ports.Outbound.IIdentityGenerator, HenryTires.Inventory.Infrastructure.Adapters.Identity.MongoIdentityGenerator>();

        services.AddSingleton<HenryTires.Inventory.Application.Common.ITimezoneConverter, TimezoneConverter>();

        // Domain Services
        services.AddSingleton<StockAvailabilityService>();
        services.AddSingleton<PriceResolutionService>();

        // Application Services (registered in Application layer)
        services.AddApplicationServices();

        return services;
    }
}
