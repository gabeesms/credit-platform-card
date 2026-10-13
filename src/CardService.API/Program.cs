using System.Text.Json.Serialization;
using CardService.Application.Events;
using CardService.Application.Interfaces;
using CardService.Application.UseCases;
using CardService.Infrastructure.Messaging;
using CardService.Infrastructure.Persistence;
using CardService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("CardService")
    ?? throw new InvalidOperationException("ConnectionStrings:CardService is required.");

builder.Services.AddDbContext<CardDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<ICardRepository, CardRepository>();
builder.Services.AddScoped<CreditApprovedEventHandler>();
builder.Services.AddScoped<GetCardHandler>();
builder.Services.AddScoped<GetCustomerCardsHandler>();
builder.Services.AddScoped<ChangeCardStatusHandler>();
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHostedService<CreditApprovedConsumerService>();
}

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(
        "/swagger",
        options => options.WithTitle("Card Service API"));
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program;
