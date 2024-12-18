using GoodBadHabitsTracker.Application.Extensions;
using GoodBadHabitsTracker.Infrastructure.Extensions;
using GoodBadHabitsTracker.WebApi.Middleware;
using Serilog;
using Newtonsoft.Json;
using Microsoft.AspNetCore.ResponseCompression;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using GoodBadHabitsTracker.WebApi.Filters;
using DateOnlyTimeOnly.AspNet.Converters;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

builder.Host.UseSerilog((hostingContext, services, loggerConfiguration) =>
{
    loggerConfiguration
    .ReadFrom.Configuration(hostingContext.Configuration)
    .ReadFrom.Services(services);
});

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var containerBuilder = new ContainerBuilder();
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        containerBuilder.BuildMediator();
        containerBuilder.BuildAutoMapper();
        containerBuilder.BuildCustomServices();
    });

builder.Services.AddConfigurations(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<GzipCompressionProvider>();
    options.EnableForHttps = true;
});
builder.Services.AddOutputCache();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
         options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
         options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    });

builder.Services.AddDateOnlyTimeOnlyStringConverters();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = false;
});
builder.Services.AddSwaggerGen(x =>
{
    x.UseDateOnlyTimeOnlyStringConverters();
    x.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter your JWT token in this field",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
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
    .AddMvc()
    .AddNewtonsoftJson();

builder.Services.AddCors(options =>
{ 
    options.AddDefaultPolicy(b =>
    {
        var allowedOrigins = builder.Configuration["CorsSettings:AllowedOrigins"];
        if (string.IsNullOrEmpty(allowedOrigins))
        {
            throw new ArgumentNullException("CorsSettings:AllowedOrigins", "AllowedOrigins cannot be null or empty.");
        }
        b.WithOrigins(allowedOrigins.Split(',')).AllowCredentials().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("Set-Cookie");
    });
});


var app = builder.Build();
app.UseOutputCache();
app.UseExceptionHandlingMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseResponseCompression();
app.UseHsts();
app.UseHttpsRedirection();
app.MapControllers();
app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.Run();

