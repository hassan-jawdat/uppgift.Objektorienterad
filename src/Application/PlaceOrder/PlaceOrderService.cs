using Objektorienterad.Application.Abstractions;
using Objektorienterad.Domain.Common;
using Objektorienterad.Domain.Orders;
using Objektorienterad.Domain.Pricing;

namespace Objektorienterad.Application.PlaceOrder;

public sealed record PlaceOrderItem(string ProductId, int Quantity);
public sealed record PlaceOrderCommand(string CustomerId, string CustomerType, IReadOnlyCollection<PlaceOrderItem> Items);
public sealed record PlaceOrderResult(bool Success, string? ErrorCode, OrderId? OrderId);

public sealed class PlaceOrderService
{
    private readonly IProductCatalog _productCatalog;
    private readonly IOrderRepository _orderRepository;
    private readonly IDiscountStrategy _discountStrategy;
    private readonly OrderFactory _orderFactory;

    public PlaceOrderService(
        IProductCatalog productCatalog,
        IOrderRepository orderRepository,
        IDiscountStrategy discountStrategy,
        OrderFactory orderFactory)
    {
        _productCatalog = productCatalog;
        _orderRepository = orderRepository;
        _discountStrategy = discountStrategy;
        _orderFactory = orderFactory;
    }

    public async Task<PlaceOrderResult> HandleAsync(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        if (command.Items.Count == 0)
        {
            return new PlaceOrderResult(false, "EMPTY_ORDER", null);
        }

        var reservations = command.Items
            .GroupBy(x => x.ProductId, StringComparer.OrdinalIgnoreCase)
            .Select(g => new InventoryReservation(g.First().ProductId, g.Sum(x => x.Quantity)))
            .ToArray();

        var ids = reservations.Select(x => x.ProductId);
        var products = await _productCatalog.GetProductsAsync(ids, cancellationToken);
        var productMap = products.ToDictionary(p => p.ProductId, StringComparer.OrdinalIgnoreCase);

        foreach (var reservation in reservations)
        {
            if (!productMap.TryGetValue(reservation.ProductId, out var product))
            {
                return new PlaceOrderResult(false, "PRODUCT_NOT_FOUND", null);
            }

            if (reservation.Quantity > product.AvailableQuantity)
            {
                return new PlaceOrderResult(false, "OUT_OF_STOCK", null);
            }
        }

        var orderItems = command.Items
            .Select(item => new OrderItem(
                item.ProductId,
                item.Quantity,
                new Money(productMap[item.ProductId].UnitPrice, productMap[item.ProductId].Currency)))
            .ToList();

        var subtotal = Money.Zero(orderItems[0].UnitPrice.Currency);
        foreach (var oi in orderItems)
        {
            subtotal = subtotal.Add(oi.LineTotal());
        }

        var discount = _discountStrategy.Calculate(command.CustomerType, subtotal);

        var order = _orderFactory.Create(command.CustomerId, orderItems, discount);
        order.Place();

        var reserved = await _productCatalog.ReserveAsync(reservations, cancellationToken);
        if (!reserved)
        {
            return new PlaceOrderResult(false, "OUT_OF_STOCK", null);
        }

        await _orderRepository.SaveAsync(order, cancellationToken);

        return new PlaceOrderResult(true, null, order.Id);
    }
}
