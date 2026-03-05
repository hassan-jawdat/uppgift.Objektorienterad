using Objektorienterad.Domain.Common;

namespace Objektorienterad.Domain.Pricing;

public sealed class LoyalCustomerDiscountStrategy : IDiscountStrategy
{
    private readonly decimal _rate;

    public LoyalCustomerDiscountStrategy(decimal rate = 0.10m)
    {
        if (rate < 0m || rate > 1m)
        {
            throw new ArgumentOutOfRangeException(nameof(rate));
        }

        _rate = rate;
    }

    public Money Calculate(string customerType, Money subtotal)
    {
        if (!string.Equals(customerType, "Loyal", StringComparison.OrdinalIgnoreCase))
        {
            return Money.Zero(subtotal.Currency);
        }

        return new Money(decimal.Round(subtotal.Amount * _rate, 2), subtotal.Currency);
    }
}
