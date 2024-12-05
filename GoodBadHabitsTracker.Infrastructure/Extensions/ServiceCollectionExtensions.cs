using GoodBadHabitsTracker.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Infrastructure.Repositories;
using GoodBadHabitsTracker.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using GoodBadHabitsTracker.Infrastructure.Security.TokenHandler;
using Quartz;
using GoodBadHabitsTracker.Infrastructure.BackgroundJobs;
using GoodBadHabitsTracker.Infrastructure.Utils;


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
            services.AddIdentityCore<User>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+/ ";
            })
                .AddEntityFrameworkStores<HabitsDbContext>()
                .AddUserStore<UserStore<User, UserRole, HabitsDbContext, Guid>>()
                .AddRoles<UserRole>()
                .AddRoleStore<RoleStore<UserRole, HabitsDbContext, Guid>>()
                .AddDefaultTokenProviders();



            services.AddScoped<SignInManager<User>>();
            services.AddScoped<UserManager<User>>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IUserAccessor, UserAccessor>();
            services.AddScoped<IHabitsRepository, HabitsRepository>();
            services.AddScoped<IGroupsRepository, GroupsRepository>();
            services.AddScoped<ITokenHandler, TokenHandler>();
            services.AddScoped<IIdTokenHandler, Security.IdTokenHandler.Handler>();

            services.AddQuartz(options =>
            {
                var jobKey = JobKey.Create("UpdatePastDayResult");

                options.AddJob<UpdatePastDayResult>(jobKey)
                .AddTrigger(trigger => trigger
                        .ForJob(jobKey)
                        .StartNow()
                       /* .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(2, 30))*/);

            });

            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });
            

            services.AddJwt(configuration);
            services.AddAuthorization(options =>
            {
                options.AddPolicy("GBHTPolicy", policy =>
                {
                    policy.RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("PasswordLogin")
                    .AddAuthenticationSchemes("Auth0Login");
                });
            });
        }
    }
}