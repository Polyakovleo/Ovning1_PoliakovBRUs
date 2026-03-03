using System.Text.Json.Serialization;

namespace BankRUs.Application.UseCases.Customers;

public record BankAccountDto(
    Guid Id,
    [property: JsonPropertyName("bank-account-number")] string AccountNumber,
    decimal Balance
);

public record CustomerDetailDto(
    Guid Id,
    [property: JsonPropertyName("firstName")] string Name,
    string Email,
    string PersonalNumber,
    [property: JsonPropertyName("bank-accounts")] IReadOnlyList<BankAccountDto> BankAccounts
);

