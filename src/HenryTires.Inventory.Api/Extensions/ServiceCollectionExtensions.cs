using System.Text;
using HenryTires.Inventory.Api.Converters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace HenryTires.Inventory.Api.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers JSON converters that apply the X-Timezone-Offset header
    /// to all DateTime fields in API responses.
    /// </summary>
    public static IServiceCollection AddTimezoneOffsetJsonConverters(this IServiceCollection services)
    {
        services.AddSingleton<IConfigureOptions<JsonOptions>>(sp =>
        {
            var accessor = sp.GetRequiredService<IHttpContextAccessor>();
            return new ConfigureOptions<JsonOptions>(options =>
            {
                options.JsonSerializerOptions.Converters.Insert(0, new UtcToOffsetDateTimeConverter(accessor));
                options.JsonSerializerOptions.Converters.Insert(1, new UtcToOffsetNullableDateTimeConverter(accessor));
            });
        });
        return services;
    }
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var key =
            configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key not configured");
        var issuer =
            configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer not configured");
        var audience =
            configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience not configured");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ClockSkew = TimeSpan.Zero,
                };
            });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "Henry's Tires Inventory API",
                    Version = "v1",
                    Description = "REST API for managing tire inventory across multiple branches",
                }
            );

            // Add JWT Authentication
            c.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Description =
                        "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                }
            );

            c.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                        },
                        Array.Empty<string>()
                    },
                }
            );
        });
        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowAll",
                builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
            );
        });

        return services;
    }
}
