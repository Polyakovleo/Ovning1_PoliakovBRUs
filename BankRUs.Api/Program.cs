
using BankRUs.Application.Common.Exceptions;
using BankRUs.Application.Interfaces;
using BankRUs.Infrastructure.Email;
using BankRUs.Application.UseCases.Accounts;
using BankRUs.Application.UseCases.Customers;
using BankRUs.Infrastructure.Persistence;
using BankRUs.Infrastructure.Repositories;
using BankRUs.Infrastructure.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using BankRUs.Api;
using BankRUs.Api.Auth;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<BankDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

builder.Services.AddScoped<IAccountRepository, AccountRepository>();

builder.Services.AddScoped<IAccountNumberGenerator, AccountNumberGenerator>();

builder.Services.AddScoped<CreateCustomerWithAccount>();
builder.Services.AddScoped<CreateAccountForExistingCustomer>();

builder.Services.AddScoped<GetAllCustomers>();
builder.Services.AddScoped<GetCustomersPage>();
builder.Services.AddScoped<GetCustomerById>();
builder.Services.AddScoped<UpdateCustomerDetails>();
builder.Services.AddScoped<CloseCustomerAccount>();
builder.Services.AddScoped<OpenBankAccount>();

// Email sender
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

// Unit of Work = тот же DbContext
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<BankDbContext>());

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Query params options (pagination limits etc.)
builder.Services.Configure<QueryParamsOptions>(builder.Configuration.GetSection("QueryParams"));

// AuthN/AuthZ (simple header-based auth for homework)
builder.Services.AddAuthentication(HeaderAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, HeaderAuthenticationHandler>(
        HeaderAuthenticationHandler.SchemeName,
        _ => { });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        var (status, title, type, detail) = ex switch
        {
            DomainValidationException dve => (
                StatusCodes.Status400BadRequest,
                "Validation error",
                "https://httpstatuses.com/400",
                dve.Message),

            ArgumentException or ArgumentOutOfRangeException => (
                StatusCodes.Status400BadRequest,
                "Validation error",
                "https://httpstatuses.com/400",
                ex?.Message ?? "Validation error"),

            DbUpdateException dbEx
                when dbEx.InnerException?.Message.Contains("IX_Customers_Email") == true
                  || dbEx.InnerException?.Message.Contains("IX_Customers_PersonalNumber") == true
                => (
                    StatusCodes.Status400BadRequest,
                    "Duplicate customer",
                    "https://httpstatuses.com/400",
                    "Customer with this email or personal number already exists."),

            NotFoundException => (
                StatusCodes.Status404NotFound,
                "Not found",
                "https://httpstatuses.com/404",
                ex?.Message ?? "Resource not found"),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Server error",
                "https://httpstatuses.com/500",
                ex?.Message ?? "Unexpected server error")
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = type,
            Detail = detail,
            Instance = context.Request.Path
        };

        // полезно для дебага/логов
        problem.Extensions["traceId"] = context.TraceIdentifier;

        await context.Response.WriteAsJsonAsync(problem);
    });
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
