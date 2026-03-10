using HenryTires.Inventory.Application.Ports.Inbound;
using HenryTires.Inventory.Application.UseCases.Auth;
using HenryTires.Inventory.Application.UseCases.Dashboard;
using HenryTires.Inventory.Application.UseCases.Groups;
using HenryTires.Inventory.Application.UseCases.Inventory;
using HenryTires.Inventory.Application.UseCases.PurchaseOrders;
using HenryTires.Inventory.Application.UseCases.Reports;
using HenryTires.Inventory.Application.UseCases.Roles;
using HenryTires.Inventory.Application.UseCases.Sales;
using HenryTires.Inventory.Application.UseCases.Users;
using Microsoft.Extensions.DependencyInjection;

namespace HenryTires.Inventory.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<INewTransactionService, NewTransactionService>();
        services.AddScoped<IItemManagementService, ItemManagementService>();
        services.AddScoped<IPriceManagementService, PriceManagementService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();

        return services;
    }
}
