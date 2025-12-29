using HenryTires.Inventory.Application.Ports.Inbound;
using HenryTires.Inventory.Application.UseCases.Auth;
using HenryTires.Inventory.Application.UseCases.Inventory;
using HenryTires.Inventory.Application.UseCases.Sales;
using HenryTires.Inventory.Application.UseCases.Users;
using Microsoft.Extensions.DependencyInjection;

namespace HenryTires.Inventory.Application.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register Application layer services (Use Cases / Inbound Ports)
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register use cases as implementations of inbound ports
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<INewTransactionService, NewTransactionService>();
        services.AddScoped<IItemManagementService, ItemManagementService>();
        services.AddScoped<IPriceManagementService, PriceManagementService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
