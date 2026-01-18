namespace MyRealEstate.Domain.ValueObjects;

// Prevent mixing up currencies.
// Can add logic (e.g., currency conversion, formatting)
public class Money
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "TND"; // Default to Tunisian Dinar

    private Money() { } // For EF Core

    public Money(decimal amount, string currency = "TND")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Money other)
            return false;

        return Amount == other.Amount && Currency == other.Currency;
    }

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);

    public override string ToString() => $"{Amount:N2} {Currency}";
}
