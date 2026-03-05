using Objektorienterad.Domain.Common;
using Objektorienterad.Domain.Orders;

namespace Objektorienterad.Domain.Pricing;

public sealed class OrderFactory
{
    public Order Create(string customerId, IEnumerable<OrderItem> items, Money discount)
    {
        var order = new Order(OrderId.New(), customerId, items, discount);
        return order;
    }
}
