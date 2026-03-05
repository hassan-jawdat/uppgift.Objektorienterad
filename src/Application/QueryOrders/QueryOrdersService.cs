using Objektorienterad.Application.Abstractions;
using Objektorienterad.Domain.Orders;

namespace Objektorienterad.Application.QueryOrders;

public sealed record QueryOrdersResult(OrderId OrderId, string CustomerId, OrderStatus Status, decimal TotalAmount, string Currency);

public sealed class QueryOrdersService
{
    private readonly IOrderRepository _orderRepository;

    public QueryOrdersService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IReadOnlyCollection<QueryOrdersResult>> HandleAsync(OrderStatus? status, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.ListByStatusAsync(status, cancellationToken);

        return orders
            .Select(order =>
            {
                var total = order.Total();
                return new QueryOrdersResult(order.Id, order.CustomerId, order.Status, total.Amount, total.Currency);
            })
            .ToArray();
    }
}
