using LvlUp.Api;
using LvlUp.Api.Extensions;
using LvlUp.Application;
using LvlUp.Infrastructure;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(context.Configuration));

builder.Services
    .AddApplication()
    .AddPresentation()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddEndpoints(typeof(Program).Assembly);

WebApplication app = builder.Build();

app.MapEndpoints();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
{
    app.MapOpenApi();
}

if (app.Environment.IsEnvironment("Local"))
{
    await app.ApplyMigrationsAsync();
}

app.UseExceptionHandler();

app.UseSerilogRequestLogging();

app.UseCors(LvlUp.Api.DependencyInjection.CorsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

await app.RunAsync();
