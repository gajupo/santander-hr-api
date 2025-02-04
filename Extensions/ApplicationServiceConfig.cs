using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace santander_hr_api.Extensions
{
    public static class ApplicationServiceConfig
    {
        public static void ConfigureSerilog(this WebApplicationBuilder builder)
        {
            builder.Services.AddSerilog(
                (services, lc) =>
                    lc
                        .ReadFrom.Configuration(builder.Configuration)
                        .ReadFrom.Services(services)
                        .Enrich.FromLogContext()
                        .WriteTo.Console()
            );
        }

        public static void UseGlobalExceptionHandler(this WebApplication app)
        {
            // global unhlandel exception in case some controller or action does not handle some error
            app.UseExceptionHandler(a =>
                a.Run(async context =>
                {
                    var exceptionHandlerPathFeature =
                        context.Features.Get<IExceptionHandlerPathFeature>();
                    var exception = exceptionHandlerPathFeature?.Error;

                    Log.Error(exception, "An unhandled exception has occurred");

                    ProblemDetails problemDetails =
                        new()
                        {
                            Title = exception?.Message,
                            Status = StatusCodes.Status500InternalServerError,
                            Detail = exception?.StackTrace,
                            Type = "GlobalExceptionHandler"
                        };

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/problem+json";

                    await context.Response.WriteAsJsonAsync(problemDetails);
                })
            );
        }
    }
}
