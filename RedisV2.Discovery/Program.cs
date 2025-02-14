using RedisV2.Discovery.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies();

var app = builder.Build();

app.MapControllers();

await app.RunAsync("http://*:5000");