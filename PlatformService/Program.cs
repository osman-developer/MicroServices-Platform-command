using Microsoft.EntityFrameworkCore;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.SyncDataServices.Grpc;
using PlatformService.SyncDataServices.Http;

var builder = WebApplication.CreateBuilder (args);

//if (builder.Environment.IsProduction ()) {
//    Console.WriteLine ("--->Using in SQLServer DB");
builder.Services.AddDbContext<AppDbContext> (options =>
    options.UseSqlServer (builder.Configuration.GetConnectionString ("PlatformsConn"), sqlServerOptionsAction : sqlOptions => {
        sqlOptions.EnableRetryOnFailure (
            maxRetryCount: 300,
            maxRetryDelay: TimeSpan.FromSeconds (1),
            errorNumbersToAdd: null);
    }));
//}

//if (builder.Environment.IsDevelopment ()) {
//    Console.WriteLine ("--->Using in Memory DB");
//    builder.Services.AddDbContext<AppDbContext> (options =>
//        options.UseInMemoryDatabase ("InMem"));
//}

builder.Services.AddScoped<IPlatformRepo, PlatformRepo> ();
builder.Services.AddHttpClient<ICommandDataClient, CommandDataClient> ();
builder.Services.AddSingleton<IMessageBusClient, MessageBusClient> ();

builder.Services.AddControllers ();
builder.Services.AddAutoMapper (AppDomain.CurrentDomain.GetAssemblies ());

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer ();
builder.Services.AddSwaggerGen ();
builder.Services.AddGrpc ();

var app = builder.Build ();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment ()) {
    app.UseSwagger ();
    app.UseSwaggerUI ();
}

app.UseHttpsRedirection ();

app.UseAuthorization ();

app.MapControllers ();
app.MapGrpcService<GrpcPlatformService> ();
app.MapGet ("/protos/platforms.proto", async context => {
    await context.Response.WriteAsync (File.ReadAllText ("Protos/platforms.proto"));
});
PrepDb.PrepPopulation (app, builder.Environment.IsProduction ());

app.Run ();