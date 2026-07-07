using BuildingBlocks.Domain.Interfaces;

namespace Order.DomainEvents;

public class OrderCreatedDomainEvent : IDomainEvent
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public decimal Total { get; }
    public DateTime OccurredOn { get; }
    
    public OrderCreatedDomainEvent(Guid orderId, Guid customerId, decimal total)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Total = total;
        OccurredOn = DateTime.UtcNow;
    }
}