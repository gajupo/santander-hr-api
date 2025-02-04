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
    }
}
