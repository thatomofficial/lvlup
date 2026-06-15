using System.Text.Json.Serialization;
using LvlUp.Api.Endpoints.App;
using LvlUp.Api.Infrastructure;

namespace LvlUp.Api;

public static class DependencyInjection
{
    public const string CorsPolicyName = "Frontend";

    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddOptions<AppVersionOptions>().BindConfiguration(AppVersionOptions.SectionName);

        services.AddOpenApi();

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddCors(options => options.AddPolicy(CorsPolicyName, policy =>
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

        return services;
    }
}
