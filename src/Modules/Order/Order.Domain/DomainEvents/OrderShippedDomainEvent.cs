using BuildingBlocks.Domain.Interfaces;

namespace Order.DomainEvents;

public class OrderShippedDomainEvent : IDomainEvent
{

    public Guid OrderId { get; set; }
    public DateTime ShippedAt { get; set; }
    public DateTime OccurredOn { get; }

    public OrderShippedDomainEvent(Guid orderId, DateTime shippedAt)
    {
        OrderId = orderId;
        ShippedAt = shippedAt;
        OccurredOn = DateTime.UtcNow;
    }
}
