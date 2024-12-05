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
using GoodBadHabitsTracker.Application.Commands;
using GoodBadHabitsTracker.Application.Queries;

using Amazon.S3;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using GoodBadHabitsTracker.Infrastructure.Configurations;
using Amazon;
using GoodBadHabitsTracker.Application.Queries.Generic.GetAll;
using GoodBadHabitsTracker.Application.Commands.Auth.DeleteAccount;
using FluentValidation;
using GoodBadHabitsTracker.Application.Commands.Habits.Create;
using GoodBadHabitsTracker.Application.Commands.Groups.Create;
using GoodBadHabitsTracker.Application.Commands.Habits.DeleteAll;
using GoodBadHabitsTracker.Application.Commands.Habits.DeleteAllProgress;
using GoodBadHabitsTracker.Application.Commands.Habits.PostInProgressToday;

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
/*            builder.RegisterGeneric(typeof(ReadByIdQueryHandler<>)).AsImplementedInterfaces();
            builder.RegisterGeneric(typeof(GetAllQueryHandler<>)).AsImplementedInterfaces();
            builder.RegisterGeneric(typeof(SearchQueryHandler<>)).AsImplementedInterfaces();
            builder.RegisterGeneric(typeof(InsertCommandHandler<,>)).AsImplementedInterfaces();
            builder.RegisterGeneric(typeof(UpdateCommandHandler<>)).AsImplementedInterfaces();
            builder.RegisterGeneric(typeof(DeleteCommandHandler<>)).AsImplementedInterfaces();
            builder.RegisterGeneric(typeof(SearchQueryValidator<>)).As(typeof(IValidator<>)).InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(InsertCommandValidator<,>)).As(typeof(IValidator<>)).InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(UpdateCommandValidator<>)).As(typeof(IValidator<>)).InstancePerLifetimeScope();*/
            builder.RegisterType<LoginCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<RegisterCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<ConfirmEmailCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<RefreshTokenCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<ForgetPasswordCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<ResetPasswordCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<ExternalLoginCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<GetExternalTokensQueryHandler>().AsImplementedInterfaces();
            builder.RegisterType<DeleteAccountCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<Commands.Habits.Create.CreateCommandValidator>().AsImplementedInterfaces();
            builder.RegisterType<Commands.Groups.Create.CreateCommandValidator>().AsImplementedInterfaces();
            builder.RegisterType<Commands.Habits.Update.UpdateCommandValidator>().AsImplementedInterfaces();
            builder.RegisterType<Commands.Groups.Update.UpdateCommandValidator>().AsImplementedInterfaces();
            builder.RegisterType<Queries.Habits.ReadAll.ReadAllQueryHandler>().AsImplementedInterfaces();
            builder.RegisterType<Queries.Habits.ReadById.ReadByIdQueryHandler>().AsImplementedInterfaces();
            builder.RegisterType<Queries.Habits.Search.SearchQueryHandler>().AsImplementedInterfaces();
            builder.RegisterType<Commands.Habits.Create.CreateCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<Commands.Habits.Update.UpdateCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<Commands.Habits.Delete.DeleteCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<Queries.Groups.ReadAll.ReadAllQueryHandler>().AsImplementedInterfaces();
            builder.RegisterType<Queries.Groups.ReadById.ReadByIdQueryHandler>().AsImplementedInterfaces();
            builder.RegisterType<Commands.Groups.Create.CreateCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<Commands.Groups.Update.UpdateCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<DeleteAllProgressCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<Commands.Groups.Delete.DeleteCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<DeleteAllCommandHandler>().AsImplementedInterfaces();
            builder.RegisterType<PostInProgressTodayCommandHandler>().AsImplementedInterfaces();
            return builder;
        }

        public static ContainerBuilder BuildAutoMapper(this ContainerBuilder builder)
        {
            builder.Register<AutoMapper.IConfigurationProvider>(ctx => new MapperConfiguration(cfg => cfg.AddProfile(typeof(HabitsMappingProfile))))
                .SingleInstance();
            builder.Register<IMapper>(ctx => new Mapper(ctx.Resolve<AutoMapper.IConfigurationProvider>(), ctx.Resolve))
                .InstancePerDependency();

            return builder;
        }
    }
}
