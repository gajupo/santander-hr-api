using santander_hr_api.Config;
using santander_hr_api.Infraestructure;
using santander_hr_api.Interfaces;
using santander_hr_api.Services;

namespace santander_hr_api.Extensions
{
    public static class DIApplicationConfig
    {
        public static void ConfigureDependencies(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IHackerNewsClient, HackerNewsClient>();
            builder.Services.AddScoped<IHackerNewsService, HackerNewsService>();
        }

        public static void ConfigureMappings(this WebApplicationBuilder builder)
        {
            // register mapper profile
            builder.Services.AddAutoMapper(typeof(MappingProfile));
        }
    }
}
