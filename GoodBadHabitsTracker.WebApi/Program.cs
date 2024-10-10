using GoodBadHabitsTracker.Application.Extensions;
using GoodBadHabitsTracker.Infrastructure.Extensions;
using GoodBadHabitsTracker.WebApi.Middleware;
using Serilog;
using Newtonsoft.Json;
using Microsoft.AspNetCore.ResponseCompression;
using GoodBadHabitsTracker.WebApi.Conventions;
using GoodBadHabitsTracker.WebApi.Utils;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;


var builder = WebApplication.CreateBuilder(args);
var containerBuilder = new ContainerBuilder();

builder.Host.UseSerilog((hostingContext, services, loggerConfiguration) =>
{
    loggerConfiguration
    .ReadFrom.Configuration(hostingContext.Configuration)
    .ReadFrom.Services(services);
});

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        containerBuilder.BuildMediator();
        containerBuilder.BuildAutoMapper();
    });

builder.Services.AddOptions();
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<GzipCompressionProvider>();
    options.EnableForHttps = true;
});
builder.Services.AddOutputCache()
    .AddStackExchangeRedisCache(options =>
    {
        options.InstanceName = "GoodBadHabitsTracker";
        options.Configuration = "localhost:6379";
    });

builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });
builder.Services.AddDateOnlyTimeOnlyStringConverters();
builder.Services.AddHttpContextAccessor();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x =>
{
    x.UseDateOnlyTimeOnlyStringConverters();
    x.AddSecurityDefinition("GHBTAuth", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    x.OperationFilter<SecurityRequirementsOperationFilter>();
});

var mvcBuilder = builder.Services
    .AddLogging()
    .AddMvc()
    .AddNewtonsoftJson();

mvcBuilder.AddMvcOptions(o =>
{
    o.Conventions.Add(new GenericControllerConventions());
    var formatter = o.InputFormatters.OfType<NewtonsoftJsonInputFormatter>().First();
});
mvcBuilder.ConfigureApplicationPartManager(c =>
{
    c.FeatureProviders.Add(new GenericControllerFeatureProvider());
});

var app = builder.Build();
app.UseOutputCache();

app.UseExceptionHandlingMiddleware();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseResponseCompression();
app.UseHsts();
app.UseHttpsRedirection();

app.MapControllers();
app.UseRouting();


app.UseAuthentication();

app.UseAuthorization();

app.Run();

public partial class Program { }