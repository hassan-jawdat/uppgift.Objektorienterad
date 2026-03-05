using Objektorienterad.Domain.Common;

namespace Objektorienterad.Domain.Orders;

public sealed class Order
{
    private readonly List<OrderItem> _items = new();
    private readonly List<IDomainEvent> _events = new();

    public OrderId Id { get; }
    public string CustomerId { get; }
    public OrderStatus Status { get; private set; }
    public Money Discount { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _events.AsReadOnly();

    public Order(OrderId id, string customerId, IEnumerable<OrderItem> items, Money discount)
    {
        Id = id;
        CustomerId = string.IsNullOrWhiteSpace(customerId)
            ? throw new ArgumentException("CustomerId is required.", nameof(customerId))
            : customerId;

        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        _items.AddRange(items);

        if (_items.Count == 0)
        {
            throw new InvalidOperationException("Order must contain at least one item.");
        }

        Discount = discount;
        Status = OrderStatus.Draft;
    }

    public Money Subtotal()
    {
        var currency = _items[0].UnitPrice.Currency;
        var subtotal = Money.Zero(currency);

        foreach (var item in _items)
        {
            subtotal = subtotal.Add(item.LineTotal());
        }

        return subtotal;
    }

    public Money Total() => Subtotal().Subtract(Discount);

    public void Place()
    {
        if (Status != OrderStatus.Draft)
        {
            throw new InvalidOperationException("Only draft orders can be placed.");
        }

        Status = OrderStatus.Placed;
        _events.Add(new OrderPlaced(Id));
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Cancelled)
        {
            return;
        }

        if (Status != OrderStatus.Draft && Status != OrderStatus.Placed)
        {
            throw new InvalidOperationException("Only draft or placed orders can be cancelled.");
        }

        Status = OrderStatus.Cancelled;
    }

    public void ClearDomainEvents() => _events.Clear();
}
