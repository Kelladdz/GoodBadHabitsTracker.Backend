using GoodBadHabitsTracker.Application.Extensions;
using GoodBadHabitsTracker.Infrastructure.Extensions;
using GoodBadHabitsTracker.WebApi.Middleware;
using Serilog;
using Newtonsoft.Json;
using Microsoft.AspNetCore.ResponseCompression;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using GoodBadHabitsTracker.Infrastructure.Configurations;
using GoodBadHabitsTracker.WebApi.Filters;


var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((hostingContext, services, loggerConfiguration) =>
{
    loggerConfiguration
    .ReadFrom.Configuration(hostingContext.Configuration)
    .ReadFrom.Services(services);
});
var containerBuilder = new ContainerBuilder();
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        containerBuilder.BuildMediator();
        containerBuilder.BuildAutoMapper();
        containerBuilder.BuildCustomServices();
    });
builder.Services.AddOptions();
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
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
builder.Services.AddHttpClient();
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
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = false;
});
builder.Services.AddSwaggerGen(x =>
{
    x.UseDateOnlyTimeOnlyStringConverters();
    x.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter your JWT token in this field",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    var securityRequirement = new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            []
        }
    };

    x.AddSecurityRequirement(securityRequirement);
    x.OperationFilter<SecurityRequirementsOperationFilter>();
    x.OperationFilter<AuthenticationRequirementsOperationFilter>();
});

var mvcBuilder = builder.Services
    .AddLogging()
    .AddMvc()
    .AddNewtonsoftJson();

/*mvcBuilder.AddMvcOptions(o =>
{
    o.Conventions.Add(new GenericControllerConventions());
    var formatter = o.InputFormatters.OfType<NewtonsoftJsonInputFormatter>().First();
});
mvcBuilder.ConfigureApplicationPartManager(c =>
{
    c.FeatureProviders.Add(new GenericControllerFeatureProvider());
});*/
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("https://localhost:8080").AllowCredentials().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("Set-Cookie");
    });
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
app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.Run();

