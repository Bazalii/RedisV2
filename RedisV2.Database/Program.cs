using RedisV2.Database.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddDependencies();

var app = builder.Build();

app.MapControllers();

app.MapHealthChecks("/health-check");

var port = builder.Configuration.GetValue<int>("ServiceSettings:Port");

await app.RunAsync($"http://*:{port}");