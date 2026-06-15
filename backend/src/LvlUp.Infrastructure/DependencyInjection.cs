using System.Text;
using LvlUp.Application.Abstractions.Authentication;
using LvlUp.Application.Abstractions.Data;
using LvlUp.Application.Abstractions.Events;
using LvlUp.Application.Abstractions.Integrations;
using LvlUp.Application.Abstractions.Notifications;
using LvlUp.Application.Abstractions.Storage;
using LvlUp.Application.Hunters.GetBadges;
using LvlUp.Application.Hunters.GetConsistency;
using LvlUp.Infrastructure.Authentication;
using LvlUp.Infrastructure.DataGateways;
using LvlUp.Infrastructure.Notifications;
using LvlUp.Infrastructure.Storage;
using LvlUp.Infrastructure.Integrations;
using LvlUp.Infrastructure.Authorization;
using LvlUp.Infrastructure.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace LvlUp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddDatabase(configuration)
            .AddStorage(configuration)
            .AddIntegrations(configuration)
            .AddAuthenticationInternal(configuration)
            .AddAuthorizationInternal();

    private static IServiceCollection AddIntegrations(this IServiceCollection services, IConfiguration configuration)
    {
        string gitHubApiBaseUrl = configuration["GitHub:ApiBaseUrl"] ??
            throw new InvalidOperationException("Configuration value 'GitHub:ApiBaseUrl' is not set.");

        services.AddHttpClient<IGitHubActivityVerifier, GitHubActivityVerifier>(client =>
        {
            client.BaseAddress = new Uri(gitHubApiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("LvlUp");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        });

        services.AddOptions<GoogleSsoOptions>()
            .Bind(configuration.GetSection(GoogleSsoOptions.SectionName));

        GoogleSsoOptions googleSsoOptions =
            configuration.GetSection(GoogleSsoOptions.SectionName).Get<GoogleSsoOptions>() ?? new GoogleSsoOptions();

        services.AddHttpClient<IGoogleIdTokenVerifier, GoogleIdTokenVerifier>(client =>
        {
            client.BaseAddress = new Uri(googleSsoOptions.TokenInfoBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'Database' is not configured. " +
                "Set it via appsettings, user secrets, or the ConnectionStrings__Database environment variable.");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default))
                .UseSnakeCaseNamingConvention()
                // The naming-convention plugin makes EF report spurious pending model
                // changes (the scaffolded diff is empty); real drift is caught by
                // 'dotnet ef migrations add' in development.
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IDomainEventsDispatcher, DomainEventsDispatcher>();

        services.AddScoped<IConsistencyDataGateway, ConsistencyDataGateway>();
        services.AddScoped<IBadgeProgressDataGateway, BadgeProgressDataGateway>();

        return services;
    }

    private static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<StorageOptions>()
            .Bind(configuration.GetSection(StorageOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Root),
                "Storage:Root must be configured. Set it via appsettings or the Storage__Root environment variable.")
            .ValidateOnStart();

        services.AddSingleton<IFileStorage, LocalFileStorage>();

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Secret) && options.Secret.Length >= 32,
                "Jwt:Secret must be configured and at least 32 characters long. " +
                "Set it via user secrets or the Jwt__Secret environment variable.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Issuer),
                "Jwt:Issuer must be configured.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Audience),
                "Jwt:Audience must be configured.")
            .Validate(
                options => options.ExpirationInMinutes > 0,
                "Jwt:ExpirationInMinutes must be greater than zero.")
            .ValidateOnStart();

        JwtOptions jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                    ClockSkew = TimeSpan.Zero,
                };
            });

        services.AddHttpContextAccessor();
        services.AddSingleton<ITokenProvider, TokenProvider>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IOtpGenerator, OtpGenerator>();
        services.AddSingleton<IPasswordResetNotifier, LoggingPasswordResetNotifier>();
        services.AddScoped<IUserContext, UserContext>();

        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddAuthorization();

        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return services;
    }
}
