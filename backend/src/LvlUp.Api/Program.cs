using LvlUp.Api;
using LvlUp.Api.Extensions;
using LvlUp.Application;
using LvlUp.Application.Abstractions.Storage;
using Microsoft.Extensions.FileProviders;
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

// Serve externally stored files (avatars) from the configured storage root.
string storageRoot = Path.GetFullPath(app.Configuration["Storage:Root"] ?? "storage");
Directory.CreateDirectory(storageRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storageRoot),
    RequestPath = StoragePaths.PublicRequestPath,
});

app.UseSerilogRequestLogging();

app.UseCors(LvlUp.Api.DependencyInjection.CorsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

await app.RunAsync();
