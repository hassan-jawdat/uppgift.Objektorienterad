using Objektorienterad.Domain.Common;

namespace Objektorienterad.Domain.Orders;

public sealed class OrderPlaced : IDomainEvent
{
    public OrderId OrderId { get; }
    public DateTime OccurredUtc { get; }

    public OrderPlaced(OrderId orderId)
    {
        OrderId = orderId;
        OccurredUtc = DateTime.UtcNow;
    }
}
