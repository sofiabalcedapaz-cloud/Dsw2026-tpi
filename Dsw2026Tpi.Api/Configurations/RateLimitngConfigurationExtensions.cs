using Dsw2026Tpi.CrossCutting.Identity;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Dsw2026Tpi.Api.Configurations
{
    public static class RateLimitingConfigurationExtensions
    {
        public static IServiceCollection AddAppRateLimiting(this IServiceCollection services, IConfiguration configuration)
        {
            var section = configuration.GetSection("RateLimiting");

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddPolicy(RateLimitPolices.General, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = section.GetValue<int>("General:PermitLimit", 100),
                        Window = TimeSpan.FromSeconds(section.GetValue<int>("General:WindowSeconds", 60)),
                        QueueLimit = 0
                    }));

                options.AddPolicy(RateLimitPolices.AdminLogin, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = section.GetValue<int>("AdminLogin:PermitLimit"),
                            Window = TimeSpan.FromSeconds(section.GetValue<int>("AdminLogin:WindowSeconds")),
                            QueueLimit = 0
                        }));

                options.AddPolicy(RateLimitPolices.PatientLogin, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = section.GetValue<int>("PatientLogin:PermitLimit"),
                            Window = TimeSpan.FromSeconds(section.GetValue<int>("PatientLogin:WindowSeconds")),
                            QueueLimit = 0
                        }));

                options.AddPolicy(RateLimitPolices.AppointmentBooking, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = section.GetValue<int>("AppointmentBooking:PermitLimit"),
                            Window = TimeSpan.FromSeconds(section.GetValue<int>("AppointmentBooking:WindowSeconds")),
                            QueueLimit = 0
                        }));

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = section.GetValue<int>("General:PermitLimit"),
                            Window = TimeSpan.FromSeconds(section.GetValue<int>("General:WindowSeconds")),
                            QueueLimit = 0
                        }));
            });

            return services;
        }
    }
}
