using Dsw2026Tpi.Api.Configurations;
using Dsw2026Tpi.Api.Middlewares;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;
using Dsw2026Tpi.Data.Identity;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Identity;

namespace Dsw2026Tpi.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Iniciando aplicación Dsw2026Tpi.Api");

            var builder = WebApplication.CreateBuilder(args);

            builder.AddSerilogConfiguration();
            builder.Services.AddAppIdentity();
            builder.Services.AddAppAuthentication(builder.Configuration);
            builder.Services.AddAppRateLimiting(builder.Configuration);
            builder.Services.AddSwaggerConfiguration();
            builder.Services.AddApplicationPersistence(builder.Configuration);
            builder.Services.AddAppCors(builder.Configuration);
            builder.Services.AddAppDependencies();
            builder.Services.AddControllers();
            builder.Services.AddHealthChecks();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                var adminEmail = configuration["AdminSeed:Email"];
                var adminPassword = configuration["AdminSeed:Password"];

                if (!string.IsNullOrWhiteSpace(adminEmail) && await userManager.FindByEmailAsync(adminEmail) is null)
                {
                    var adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(adminUser, adminPassword!);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, Roles.Administrator);
                        Log.Information("Usuario administrador inicial creado: {Email}", adminEmail);
                    }
                    else
                    {
                        Log.Warning("No se pudo crear el usuario administrador inicial: {Errors}",
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }

            app.UseSerilogRequestLogging();
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsProduction())
            {
                app.UseHttpsRedirection();
            }
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();

            app.MapControllers();
            app.MapHealthChecks("/health-check");

            Log.Information("Aplicación iniciada correctamente");

            await app.RunAsync();
        }
        catch (HostAbortedException)
        {
            Log.Information("El host fue abortado (normal durante migraciones de EF Core)");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "La aplicación falló al iniciar");
            throw;
        }
        finally
        {
            Log.Information("Cerrando aplicación");
            await Log.CloseAndFlushAsync();
        }
    }
}

