using System.Windows.Input;
using CommandsService.AsyncDataServices;
using CommandsService.Data;
using CommandsService.EventProcessing;
using CommandsService.SyncDataServices.Grpc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder (args);

builder.Services.AddDbContext<AppDbContext> (options => options.UseInMemoryDatabase ("InMem"));
builder.Services.AddScoped<ICommandRepo, CommandRepo> ();
builder.Services.AddControllers ();
builder.Services.AddHostedService<MessageBusSubscriber> ();
builder.Services.AddSingleton<IEventProcessor, EventProcessor> ();
builder.Services.AddAutoMapper (AppDomain.CurrentDomain.GetAssemblies ());
builder.Services.AddScoped<IIPlatformDataClient, PlatformDataClient> ();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer ();
builder.Services.AddSwaggerGen ();

var app = builder.Build ();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment ()) {
    app.UseSwagger ();
    app.UseSwaggerUI ();
}

app.UseHttpsRedirection ();

app.UseAuthorization ();

app.MapControllers ();

PrepDb.PrepPopulation (app);

app.Run ();