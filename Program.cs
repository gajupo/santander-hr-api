using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using santander_hr_api.Extensions;
using Serilog;

namespace santander_hr_api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // create a logger configuration
            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

            Log.Information("Starting up");

            try
            {
                WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

                // configure Serilog
                builder.ConfigureSerilog();

                // add services
                builder.ConfigureDependencies();

                // add mappings
                builder.ConfigureMappings();

                // add controllers
                builder.Services.AddControllers();

                // configure the http client specific to the API base url, take it from the configuration
                builder.Services.AddHttpClient(
                    "hackerNews",
                    client =>
                    {
                        client.BaseAddress = new Uri(
                            builder.Configuration["HackerRankBaseUrl"]
                                ?? "https://hacker-news.firebaseio.com/v0/"
                        );
                        client.DefaultRequestHeaders.Add("Accept", "application/json");
                    }
                );

                // add memory cache
                builder.Services.AddMemoryCache();

                // add swagger
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                // build the application
                await using WebApplication app = builder.Build();

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

                app.UseHttpsRedirection();

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.MapControllers();

                // runn the application asynchronusly
                await app.RunAsync();

                Log.Information("Shutting down");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
