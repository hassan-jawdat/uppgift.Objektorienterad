using Objektorienterad.Application.Abstractions;
using Objektorienterad.Domain.Orders;

namespace Objektorienterad.Infrastructure.Persistence;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly Dictionary<OrderId, Order> _store = new();

    public Task SaveAsync(Order order, CancellationToken cancellationToken)
    {
        _store[order.Id] = order;
        return Task.CompletedTask;
    }

    public Task<Order?> GetByIdAsync(OrderId orderId, CancellationToken cancellationToken)
    {
        _store.TryGetValue(orderId, out var order);
        return Task.FromResult(order);
    }

    public Task<IReadOnlyCollection<Order>> ListByStatusAsync(OrderStatus? status, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Order> result = status is null
            ? _store.Values.ToArray()
            : _store.Values.Where(x => x.Status == status.Value).ToArray();

        return Task.FromResult(result);
    }
}
