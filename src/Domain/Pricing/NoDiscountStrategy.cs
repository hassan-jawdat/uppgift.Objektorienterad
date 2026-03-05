using Objektorienterad.Domain.Common;

namespace Objektorienterad.Domain.Pricing;

public sealed class NoDiscountStrategy : IDiscountStrategy
{
    public Money Calculate(string customerType, Money subtotal) => Money.Zero(subtotal.Currency);
}
