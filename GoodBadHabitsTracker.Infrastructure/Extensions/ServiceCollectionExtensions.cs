using GoodBadHabitsTracker.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Infrastructure.Security.AccessTokenHandler;
using GoodBadHabitsTracker.Infrastructure.Security.RefreshTokenHandler;

namespace GoodBadHabitsTracker.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<HabitsDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("Default")));
            services.AddIdentityCore<User>(options => options.User.RequireUniqueEmail = true)
                .AddEntityFrameworkStores<HabitsDbContext>()
                .AddUserStore<UserStore<User, UserRole, HabitsDbContext, Guid>>()
                .AddRoles<UserRole>()
                .AddRoleStore<RoleStore<UserRole, HabitsDbContext, Guid>>();

            services.AddTransient<IAccessTokenHandler, Handler>();
            services.AddTransient<IRefreshTokenHandler, RefreshTokenHandler>();

            services.AddJwt(configuration);
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Jwt", policy =>
                    policy.RequireClaim("authenticationMethod", "EmailPassword"));
            });
        }
    }
}
