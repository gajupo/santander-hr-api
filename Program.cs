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

                // configure the global exception handler
                app.UseGlobalExceptionHandler();

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
