using BankApi.Application;
using BankApi.Application.Common;
using BankApi.Application.TransferObjects;
using BankApi.Infrastructure.EntityFrameworkCore;
using BankApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddDbContext<BankDbContext>(options =>
{
    options.UseInMemoryDatabase("BankDb");
});

WebApplication app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/v1/accounts", async (IAccountService accountService, CancellationToken cancellationToken) =>
    {
       IReadOnlyCollection<AccountDto> accounts = await accountService.GetAllAccountsAsync(cancellationToken);
       return Results.Ok(accounts);
    })
    .WithOpenApi();

app.MapGet("/api/v1/accounts/search/{phone}", async (string phone, IAccountService accountService, CancellationToken cancellationToken) =>
    {
        AccountDto account =  await accountService.GetAccountByPhoneAsync(phone, cancellationToken);
        return Results.Ok(account);
    })
    .WithOpenApi();

app.MapPost("/api/v1/accounts", async (CreateAccountDto dto, IAccountService accountService, CancellationToken cancellationToken) =>
    {
        AccountDto account = await accountService.CreateAccountAsync(dto, cancellationToken);
        return Results.Ok(account);
    })
    .WithOpenApi();

app.MapPost("/api/v1/transactions", async (CreateTransactionDto dto, IAccountService accountService, CancellationToken cancellationToken) =>
    {
        await accountService.CreateTransactionAsync(dto, cancellationToken);
        return Results.Ok();
    })
    .WithOpenApi();

app.Run();