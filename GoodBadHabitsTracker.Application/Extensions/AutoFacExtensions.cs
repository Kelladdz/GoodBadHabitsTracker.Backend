using Autofac;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using MediatR.Extensions.Autofac.DependencyInjection;
using FluentValidation;
using AutoMapper;
using System.Reflection;
using GoodBadHabitsTracker.Application.Commands.Generic.Insert;
using GoodBadHabitsTracker.Application.Commands.Generic.Update;
using GoodBadHabitsTracker.Application.Queries.Generic.ReadById;
using GoodBadHabitsTracker.Application.Queries.Generic.Search;
using GoodBadHabitsTracker.Application.Mappings;
using GoodBadHabitsTracker.Application.Abstractions.Behaviors;
using GoodBadHabitsTracker.Application.Commands.Generic.Delete;
using GoodBadHabitsTracker.Application.Commands.Auth.Login;
using GoodBadHabitsTracker.Application.Commands.Auth.Register;
using GoodBadHabitsTracker.Application.Queries.Generic.GetAll;

namespace GoodBadHabitsTracker.Application.Extensions
{
    public static class AutoFacExtensions
    {
        public static ContainerBuilder BuildMediator(this ContainerBuilder builder)
        {
            var mediatrConfiguration = MediatRConfigurationBuilder.Create(Assembly.GetExecutingAssembly())
                .Build();
            builder.RegisterMediatR(mediatrConfiguration);
            builder.RegisterGeneric(typeof(ValidationBehavior<,>)).As(typeof(IPipelineBehavior<,>));
            builder.RegisterGeneric(typeof(ReadByIdQueryHandler<>)).AsImplementedInterfaces();
            builder.RegisterGeneric(typeof(GetAllQueryHandler<>)).AsImplementedInterfaces();
            builder.RegisterGeneric(typeof(SearchQueryHandler<>)).AsImplementedInterfaces();
            builder.RegisterGeneric(typeof(InsertCommandHandler<,>)).AsImplementedInterfaces();
            builder.RegisterGeneric(typeof(UpdateCommandHandler<>)).AsImplementedInterfaces();
            builder.RegisterGeneric(typeof(DeleteCommandHandler<>)).AsImplementedInterfaces();
            builder.RegisterGeneric(typeof(SearchQueryValidator<>)).As(typeof(IValidator<>)).InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(InsertCommandValidator<,>)).As(typeof(IValidator<>)).InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(UpdateCommandValidator<>)).As(typeof(IValidator<>)).InstancePerLifetimeScope();
            builder.RegisterType<LoginCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<RegisterCommandHandler>().AsImplementedInterfaces();
            return builder;
        }

        public static ContainerBuilder BuildAutoMapper(this ContainerBuilder builder)
        {
            builder.Register<IConfigurationProvider>(ctx => new MapperConfiguration(cfg => cfg.AddProfile(typeof(HabitsMappingProfile))))
                .SingleInstance();
            builder.Register<IMapper>(ctx => new Mapper(ctx.Resolve<IConfigurationProvider>(), ctx.Resolve))
                .InstancePerDependency();

            return builder;
        }
    }
}
