using HubStream.Domain.Currencies.Enums;
using System;

namespace HubStream.Domain.Currencies.Entities
{
    public record Money
    {
        public decimal Amount { get; init; }
        public Currency CurrencyCode { get; init; }

        public Money(decimal amount, Currency currencyCode)
        {
            if (amount < 0)
                throw new ArgumentException("El importe no puede ser negativo", nameof(amount));

            Amount = amount;
            CurrencyCode = currencyCode;
        }

        public static Money Zero(Currency currencyCode) => new(0m, currencyCode);

        // Operaciones aritméticas
        public Money Add(Money other)
        {
            EnsureSameCurrency(other);
            return new Money(Amount + other.Amount, CurrencyCode);
        }

        public Money Subtract(Money other)
        {
            EnsureSameCurrency(other);
            if (Amount - other.Amount < 0)
                throw new InvalidOperationException("El resultado no puede ser negativo");
            return new Money(Amount - other.Amount, CurrencyCode);
        }

        public Money Multiply(decimal factor)
        {
            if (factor < 0)
                throw new ArgumentException("El factor no puede ser negativo", nameof(factor));
            return new Money(Amount * factor, CurrencyCode);
        }

        // Comparaciones
        public bool IsGreaterThan(Money other)
        {
            EnsureSameCurrency(other);
            return Amount > other.Amount;
        }

        public bool IsLessThan(Money other)
        {
            EnsureSameCurrency(other);
            return Amount < other.Amount;
        }

        private void EnsureSameCurrency(Money other)
        {
            if (CurrencyCode != other.CurrencyCode)
                throw new InvalidOperationException("No se pueden operar cantidades de distintas monedas");
        }

        public override string ToString() => $"{Amount:N2} {CurrencyCode}";
    }
}
