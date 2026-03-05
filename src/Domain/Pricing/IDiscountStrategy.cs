using Objektorienterad.Domain.Common;

namespace Objektorienterad.Domain.Pricing;

public interface IDiscountStrategy
{
    Money Calculate(string customerType, Money subtotal);
}
