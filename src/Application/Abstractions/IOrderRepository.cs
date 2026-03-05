using Objektorienterad.Domain.Orders;

namespace Objektorienterad.Application.Abstractions;

public interface IOrderRepository
{
    Task SaveAsync(Order order, CancellationToken cancellationToken);
    Task<Order?> GetByIdAsync(OrderId orderId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Order>> ListByStatusAsync(OrderStatus? status, CancellationToken cancellationToken);
}
