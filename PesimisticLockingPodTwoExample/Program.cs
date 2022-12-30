using SharedCode.Service;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services
    .AddHostedService<PessimisticLockHostedService>();

var app = builder.Build();

app.Run();