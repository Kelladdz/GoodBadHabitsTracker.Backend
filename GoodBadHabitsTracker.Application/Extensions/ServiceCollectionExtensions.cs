using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using FluentValidation;
using GoodBadHabitsTracker.Application.Mappings;
using GoodBadHabitsTracker.Application.Abstractions.Behaviors;

namespace GoodBadHabitsTracker.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());
                config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });
            services.AddAutoMapper(typeof(HabitsMappingProfile));
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
