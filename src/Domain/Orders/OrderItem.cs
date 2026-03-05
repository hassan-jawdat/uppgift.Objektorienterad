using Objektorienterad.Domain.Common;

namespace Objektorienterad.Domain.Orders;

public sealed class OrderItem
{
    public string ProductId { get; }
    public int Quantity { get; }
    public Money UnitPrice { get; }

    public OrderItem(string productId, int quantity, Money unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            throw new ArgumentException("ProductId is required.", nameof(productId));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be > 0.");
        }

        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Money LineTotal() => new(UnitPrice.Amount * Quantity, UnitPrice.Currency);
}
