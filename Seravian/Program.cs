using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Seravian.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddConfigurations(builder.Configuration);
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});

builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddBackgroundServices();

builder.Services.AddCustomCors(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddSingletonServices();

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
app.UseStaticFiles();

app.UseAuthorization();

app.MapSignalRHubs();
app.MapControllers();
app.Run();
