using BuildingBlocks.Domain.Interfaces;

namespace Order.DomainEvents;

public class OrderCancelledDomainEvent : IDomainEvent
{
    public Guid OrderId { get; }
    public DateTime CancelledAt { get; }
    public DateTime OccurredOn { get; }

    public OrderCancelledDomainEvent(Guid orderId, DateTime cancelledAt)
    {
        OrderId = orderId;
        CancelledAt = cancelledAt;
        OccurredOn = DateTime.UtcNow;
    }
}