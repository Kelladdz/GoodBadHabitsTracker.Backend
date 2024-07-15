using GoodBadHabitsTracker.Application.Extensions;
using GoodBadHabitsTracker.Infrastructure.Extensions;
using GoodBadHabitsTracker.WebApi.Middleware;
using Serilog;
using Newtonsoft.Json;


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostingContext, services, loggerConfiguration) =>
{
    loggerConfiguration
    .ReadFrom.Configuration(hostingContext.Configuration)
    .ReadFrom.Services(services);
});

builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });
builder.Services.AddDateOnlyTimeOnlyStringConverters();
builder.Services.AddHttpContextAccessor();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x => x.UseDateOnlyTimeOnlyStringConverters());

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseExceptionHandlingMiddleware();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHsts();
app.UseHttpsRedirection();

app.MapControllers();
app.UseRouting();


app.UseAuthentication();



app.UseAuthorization();

app.Run();
