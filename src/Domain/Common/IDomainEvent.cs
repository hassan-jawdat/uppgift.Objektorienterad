namespace Objektorienterad.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredUtc { get; }
}
