using SharedCode.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder
    .Services
    .AddHostedService<PessimisticLockHostedService>();

var app = builder.Build();

app.Run();