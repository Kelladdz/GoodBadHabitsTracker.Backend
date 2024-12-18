using DateOnlyTimeOnly.AspNet.Converters;

using GoodBadHabitsTracker.Infrastructure.Settings;
using GoodBadHabitsTracker.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Controllers = Microsoft.AspNetCore.Mvc;
using MinimalApis = Microsoft.AspNetCore.Http.Json;
namespace GoodBadHabitsTracker.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<CorsSettings>(configuration.GetSection("CorsSettings"));
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<Controllers::JsonOptions>(options => options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter()));
        }
    }
}
