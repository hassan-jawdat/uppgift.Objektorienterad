namespace Objektorienterad.Application.Abstractions;

public sealed record ProductSnapshot(string ProductId, decimal UnitPrice, string Currency, int AvailableQuantity);
public sealed record InventoryReservation(string ProductId, int Quantity);

public interface IProductCatalog
{
    Task<IReadOnlyCollection<ProductSnapshot>> GetProductsAsync(IEnumerable<string> productIds, CancellationToken cancellationToken);
    Task<bool> ReserveAsync(IReadOnlyCollection<InventoryReservation> reservations, CancellationToken cancellationToken);
}
