using Microsoft.EntityFrameworkCore;
using AccelokaDb.Entities.Context;
using Acceloka.API.Services;
using MediatR;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // Set level log
    .WriteTo.File($"logs/Log-{DateTime.Now:yyyyMMdd}.txt", rollingInterval: RollingInterval.Day) 
    .CreateLogger();

builder.Host.UseSerilog(); 

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AccelokaDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddTransient<TicketService>();
builder.Services.AddTransient<BookedTicketService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(typeof(Program));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();