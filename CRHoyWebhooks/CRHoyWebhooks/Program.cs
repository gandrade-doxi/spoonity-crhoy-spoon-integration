using CRHoyWebhooks.Data;
using CRHoyWebhooks.Models;
using CRHoyWebhooks.Services;
using CRHoyWebhooks.Services.CRHoyWebhooks.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<SpoonitySettings>(builder.Configuration.GetSection("Spoonity"));
builder.Services.AddHttpClient();
builder.Services.AddScoped<ISpoonityService, SpoonityService>();

builder.Services.Configure<SendGridSettings>(builder.Configuration.GetSection("SendGrid"));
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.Configure<CrHoySettings>(builder.Configuration.GetSection("CrHoy"));
builder.Services.AddScoped<ICrHoyService, CrHoyService>();

// ✅ Agrega CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ Aplica CORS antes de Authorization
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
