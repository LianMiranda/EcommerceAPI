using BuildingBlocks.Domain.Interfaces;

namespace Order.DomainEvents;

public class OrderDeliveredDomainEvent : IDomainEvent
{
    public Guid OrderId { get; }
    public DateTime DeliveredAt { get; }
    public DateTime OccurredOn { get; }

    public OrderDeliveredDomainEvent(Guid orderId, DateTime deliveredAt)
    {
        OrderId = orderId;
        DeliveredAt = deliveredAt;
        OccurredOn = DateTime.UtcNow;
    }
}