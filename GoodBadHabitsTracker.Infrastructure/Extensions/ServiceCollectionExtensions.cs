using GoodBadHabitsTracker.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Infrastructure.Repositories;
using Hangfire;
using GoodBadHabitsTracker.Infrastructure.Services;


namespace GoodBadHabitsTracker.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<HabitsDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("Default"));
        });
            services.AddIdentityCore<User>(options => options.User.RequireUniqueEmail = true)
                .AddEntityFrameworkStores<HabitsDbContext>()
                .AddUserStore<UserStore<User, UserRole, HabitsDbContext, Guid>>()
                .AddRoles<UserRole>()
                .AddRoleStore<RoleStore<UserRole, HabitsDbContext, Guid>>();


            services.AddSingleton<IAccessTokenHandler, Security.AccessTokenHandler.Handler>();
            services.AddSingleton<IRefreshTokenHandler, Security.RefreshTokenHandler.Handler>();
            services.AddSingleton<IEmailSender, EmailSender>();

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddJwt(configuration);
            services.AddAuthorization();
        }
    }
}
