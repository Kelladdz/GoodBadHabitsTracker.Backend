using GoodBadHabitsTracker.Application.Extensions;
using GoodBadHabitsTracker.Infrastructure.Extensions;
using GoodBadHabitsTracker.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
