using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using santander_hr_api.Extensions;
using Serilog;

namespace santander_hr_api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = CreateHostBuilder(args);
            await RunApplication(builder);
        }

        public static WebApplicationBuilder CreateHostBuilder(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigureServices(builder);
            return builder;
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            // configure Serilog
            builder.ConfigureSerilog();

            // add services
            builder.ConfigureDependencies();

            // add mappings
            builder.ConfigureMappings();

            // add controllers
            builder.Services.AddControllers();

            ConfigureHttpClient(builder);

            // add memory cache
            builder.Services.AddMemoryCache();

            // add swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
        }

        private static void ConfigureHttpClient(WebApplicationBuilder builder)
        {
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
        }

        private static async Task RunApplication(WebApplicationBuilder builder)
        {
            // create a logger configuration
            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            Log.Information("Starting up");

            try
            {
                var app = builder.Build();
                await ConfigureApplication(app);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
                throw;
            }
            finally
            {
                Log.Information("Shutting down");
                Log.CloseAndFlush();
            }
        }

        private static async Task ConfigureApplication(WebApplication app)
        {
            // configure the global exception handler
            app.UseGlobalExceptionHandler();
            app.UseHttpsRedirection();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapControllers();
            await app.RunAsync();
        }
    }
}
