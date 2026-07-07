using BuildingBlocks.Domain.Entities;
using Order.CustomExceptions;

namespace Order.Entities;

public class Refund : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid? OrderItemId { get; private set; }  // null = reembolso não vinculado a item específico
    public decimal Amount { get; private set; }
    public string Reason { get; private set; } //Razão

    private Refund() {}

    public static Refund Create(Guid orderId, Guid? orderItemId, decimal amount, string reason)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be positive", nameof(amount));

        if (orderId == Guid.Empty)
            throw new DomainException("Cannot be empty", nameof(orderId));
        
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Reason cannot be empty.", nameof(reason));
        
        return new Refund
        {
            OrderId = orderId,
            OrderItemId = orderItemId,
            Amount = amount,
            Reason = reason,
        };
    }
}