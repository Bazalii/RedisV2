using RedisV2.Database.Controllers;
using RedisV2.Database.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies();

var app = builder.Build();

app.MapGrpcService<DatabaseController>();

await app.RunAsync("http://*:5000");