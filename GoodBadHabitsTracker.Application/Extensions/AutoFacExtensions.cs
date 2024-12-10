using Autofac;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection.Builder;
using MediatR.Extensions.Autofac.DependencyInjection;
using AutoMapper;
using System.Reflection;
using GoodBadHabitsTracker.Application.Mappings;
using GoodBadHabitsTracker.Application.Abstractions.Behaviors;
using GoodBadHabitsTracker.Application.Commands.Auth.Login;
using GoodBadHabitsTracker.Application.Commands.Auth.Register;
using GoodBadHabitsTracker.Application.Commands.Auth.RefreshToken;
using GoodBadHabitsTracker.Application.Commands.Auth.ForgetPassword;
using GoodBadHabitsTracker.Application.Commands.Auth.ResetPassword;
using GoodBadHabitsTracker.Application.Commands.Auth.ConfirmEmail;
using GoodBadHabitsTracker.Application.Commands.Auth.ExternalLogin;
using GoodBadHabitsTracker.Application.Queries.Auth.GetExternalTokens;
using GoodBadHabitsTracker.Application.Commands.Auth.DeleteAccount;
using GoodBadHabitsTracker.Application.Commands.Habits.DeleteAll;
using GoodBadHabitsTracker.Application.Commands.Habits.DeleteAllProgress;
using GoodBadHabitsTracker.Application.Commands.Groups.Create;
using GoodBadHabitsTracker.Application.Commands.Groups.Update;
using GoodBadHabitsTracker.Application.Commands.Habits.Delete;
using GoodBadHabitsTracker.Application.Commands.Habits.Update;
using GoodBadHabitsTracker.Application.Commands.Habits.Create;
using GoodBadHabitsTracker.Application.Queries.Habits.ReadAll;
using GoodBadHabitsTracker.Application.Queries.Habits.ReadById;
using GoodBadHabitsTracker.Application.Queries.Habits.Search;
using GoodBadHabitsTracker.Application.Queries.Groups.ReadById;
using GoodBadHabitsTracker.Application.Queries.Groups.ReadAll;
using GoodBadHabitsTracker.Application.Commands.Groups.Delete;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.Utils;


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
            builder.RegisterType<LoginCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<RegisterCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<ConfirmEmailCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<RefreshTokenCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<ForgetPasswordCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<ResetPasswordCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<ExternalLoginCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<GetExternalTokensQueryHandler>().AsImplementedInterfaces();
            builder.RegisterType<DeleteAccountCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<CreateHabitCommandValidator>().AsImplementedInterfaces();
            builder.RegisterType<CreateGroupCommandValidator>().AsImplementedInterfaces();
            builder.RegisterType<UpdateHabitCommandValidator>().AsImplementedInterfaces();
            builder.RegisterType<UpdateGroupCommandValidator>().AsImplementedInterfaces();
            builder.RegisterType<ReadAllHabitsQueryHandler>().AsImplementedInterfaces();
            builder.RegisterType<ReadHabitByIdQueryHandler>().AsImplementedInterfaces();
            builder.RegisterType<SearchHabitsQueryHandler>().AsImplementedInterfaces();
            builder.RegisterType<CreateHabitCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<UpdateHabitCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<DeleteHabitCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<ReadAllGroupsQueryHandler>().AsImplementedInterfaces();
            builder.RegisterType<ReadGroupByIdQueryHandler>().AsImplementedInterfaces();
            builder.RegisterType<CreateGroupCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<UpdateGroupCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<DeleteAllProgressCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<DeleteGroupCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<DeleteAllHabitsCommandHandler>().AsImplementedInterfaces();
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

        public static ContainerBuilder BuildCustomServices(this ContainerBuilder builder)
        {
            builder.RegisterType<UserAccessor>().AsImplementedInterfaces();
            return builder;
        }
    }
}
