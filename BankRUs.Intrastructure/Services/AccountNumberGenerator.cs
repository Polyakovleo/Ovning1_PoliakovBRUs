using System.Security.Cryptography;
using BankRUs.Application.Interfaces;

namespace BankRUs.Infrastructure.Services
{
    public class AccountNumberGenerator : IAccountNumberGenerator
    {
        private readonly IAccountRepository _accounts;

        public AccountNumberGenerator(IAccountRepository accounts)
        {
            _accounts = accounts;
        }

        public async Task<string> GenerateAsync(CancellationToken cancellationToken)
        {
            // “Хорошая” практика: ограничить попытки
            const int maxAttempts = 20;

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                // Пример формата: 12 цифр
                var candidate = Generate12DigitNumber();

                var exists = await _accounts.AccountNumberExistsAsync(candidate, cancellationToken);
                if (!exists)
                    return candidate;
            }

            throw new InvalidOperationException("Failed to generate a unique account number. Please try again.");
        }

        private static string Generate12DigitNumber()
        {
            // Генерим 12-значное число как строку, включая возможные нули в начале
            Span<byte> bytes = stackalloc byte[8];
            RandomNumberGenerator.Fill(bytes);

            var value = BitConverter.ToUInt64(bytes);

            // Берём модуль 10^12 → получаем диапазон [0..999999999999]
            var number = (long)(value % 1_000_000_000_000UL);

            return number.ToString("D12"); // 12 digits with leading zeros
        }
    }
}