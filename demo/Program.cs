using Objektorienterad.Application.Abstractions;
using Objektorienterad.Application.PlaceOrder;
using Objektorienterad.Application.QueryOrders;
using Objektorienterad.Domain.Orders;
using Objektorienterad.Domain.Pricing;
using Objektorienterad.Infrastructure.Persistence;

var catalog = new InMemoryProductCatalog(new[]
{
    new ProductSnapshot("P100", 120m, "SEK", 10),
    new ProductSnapshot("P200", 75m, "SEK", 2)
});

var repository = new InMemoryOrderRepository();
var discountStrategy = new LoyalCustomerDiscountStrategy(rate: 0.10m);
var orderFactory = new OrderFactory();

var placeOrderService = new PlaceOrderService(catalog, repository, discountStrategy, orderFactory);
var queryOrdersService = new QueryOrdersService(repository);

var successCommand = new PlaceOrderCommand(
    CustomerId: "CUST-1",
    CustomerType: "Loyal",
    Items: new[]
    {
        new PlaceOrderItem("P100", 2),
        new PlaceOrderItem("P200", 1)
    });

var successResult = await placeOrderService.HandleAsync(successCommand, CancellationToken.None);
Console.WriteLine($"Order 1 success: {successResult.Success}, code: {successResult.ErrorCode ?? "OK"}, id: {successResult.OrderId}");

var failedCommand = new PlaceOrderCommand(
    CustomerId: "CUST-2",
    CustomerType: "Regular",
    Items: new[]
    {
        new PlaceOrderItem("P200", 5)
    });

var failedResult = await placeOrderService.HandleAsync(failedCommand, CancellationToken.None);
Console.WriteLine($"Order 2 success: {failedResult.Success}, code: {failedResult.ErrorCode ?? "OK"}");

var placedOrders = await queryOrdersService.HandleAsync(OrderStatus.Placed, CancellationToken.None);
Console.WriteLine($"Placed orders count: {placedOrders.Count}");
foreach (var order in placedOrders)
{
    Console.WriteLine($"- {order.OrderId} | customer={order.CustomerId} | total={order.TotalAmount} {order.Currency}");
}

if (successResult.OrderId is { } createdOrderId)
{
    var order = await repository.GetByIdAsync(createdOrderId, CancellationToken.None);
    if (order is not null)
    {
        order.Cancel();
        Console.WriteLine($"Order {createdOrderId} cancelled. New status: {order.Status}");
    }
}

sealed class InMemoryProductCatalog : IProductCatalog
{
    private readonly Dictionary<string, ProductSnapshot> _products;

    public InMemoryProductCatalog(IEnumerable<ProductSnapshot> products)
    {
        _products = products.ToDictionary(p => p.ProductId, StringComparer.OrdinalIgnoreCase);
    }

    public Task<IReadOnlyCollection<ProductSnapshot>> GetProductsAsync(IEnumerable<string> productIds, CancellationToken cancellationToken)
    {
        var result = new List<ProductSnapshot>();
        foreach (var id in productIds)
        {
            if (_products.TryGetValue(id, out var product))
            {
                result.Add(product);
            }
        }

        return Task.FromResult<IReadOnlyCollection<ProductSnapshot>>(result);
    }

    public Task<bool> ReserveAsync(IReadOnlyCollection<InventoryReservation> reservations, CancellationToken cancellationToken)
    {
        foreach (var reservation in reservations)
        {
            if (!_products.TryGetValue(reservation.ProductId, out var existing))
            {
                return Task.FromResult(false);
            }

            if (reservation.Quantity > existing.AvailableQuantity)
            {
                return Task.FromResult(false);
            }
        }

        foreach (var reservation in reservations)
        {
            var existing = _products[reservation.ProductId];
            _products[reservation.ProductId] = existing with
            {
                AvailableQuantity = existing.AvailableQuantity - reservation.Quantity
            };
        }

        return Task.FromResult(true);
    }
}
