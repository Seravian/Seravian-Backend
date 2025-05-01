using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Cms;
using Scalar.AspNetCore;
using Seravian.Extensions;
using Seravian.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddConfigurations(builder.Configuration);
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});
builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddHostedService<CleanDbBackgroundService>();

builder.Services.AddCustomCors(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddScopedServices();
builder.Services.AddControllers();

builder.Services.AddSignalR();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "OpenAPI V1");
    });
    app.MapScalarApiReference();
}

app.UseCustomCors();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapHub<ChatHub>(
    "hubs/chat",
    options =>
    {
        options.CloseOnAuthenticationExpiration = true;
    }
);
app.MapControllers();
app.Run();
