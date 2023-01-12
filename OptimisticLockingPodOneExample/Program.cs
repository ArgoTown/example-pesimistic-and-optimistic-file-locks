using SharedCode.Service;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services
    .AddHostedService<OptimisticLockHostedService>();

var app = builder.Build();

app.Run();