using GoodBadHabitsTracker.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Quartz;
using GoodBadHabitsTracker.Infrastructure.BackgroundJobs;
using GoodBadHabitsTracker.Infrastructure.Security.JwtTokenHandler;
using GoodBadHabitsTracker.Infrastructure.Settings;
using System.IdentityModel.Tokens.Jwt;


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
            
            services.AddScoped<IJwtTokenHandler, JwtTokenHandler>();
            services.AddScoped<IHabitsDbContext, HabitsDbContext>();

            services.AddQuartz(options =>
            {
                var jobKey = JobKey.Create(QuartzJobKeys.FILL_PAST_DAYS_JOB_KEY);

                options.AddJob<FillPastDaysJob>(jobKey)
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
            services.AddAuthorizationBuilder()
                .AddPolicy(Policies.AUTHORIZATION_POLICY_NAME, policy =>
                {
                    policy.RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(AuthenticationSchemas.EMAIL_PASSWORD_AUTHENTICATION_SCHEMA)
                    .AddAuthenticationSchemes(AuthenticationSchemas.AUTH0_AUTHENTICATION_SCHEMA)
                    .Build();
                });
        }
    }
}