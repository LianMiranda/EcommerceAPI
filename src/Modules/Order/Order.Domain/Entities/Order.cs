using Order.CustomExceptions;
using Order.Enums;

namespace Order.Entities;

public class Order
{
    public int Number { get; private set; }
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public OrderStatus Status { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public decimal Discount { get; private set; }
    public decimal Subtotal => _items.Sum(i => i.TotalPrice);
    public decimal Total => Subtotal - Discount;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    private Order() {}
    
    public static Order Create(Guid customerId, decimal discount, List<OrderItem> items)
    {
        if(customerId == Guid.Empty)
            throw new DomainException("Cannot be empty",nameof(customerId));
        
        if(discount < 0)
            throw new DomainException("Discount cannot be negative", nameof(discount));
        
        if(items is null || items.Count < 1)
            throw new DomainException("Items cannot be empty", nameof(items));
        
        var order = new Order
        {
            Id = Guid.CreateVersion7(),
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            Discount = discount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            PaidAt = null,
            ShippedAt = null,
            DeliveredAt = null,
            CancelledAt = null
        };
        
        foreach (var item in items)
            order.AddItem(item);
        
        return order;
    }

    public void AddItem(OrderItem orderItem)
    {
        if(Status != OrderStatus.Pending)
            throw new DomainException($"Cannot add order item {orderItem} to order with status {Status}.");
        
        _items.Add(orderItem);
    }
    
    public void RemoveItem(OrderItem orderItem)
    {
        if(Status != OrderStatus.Pending)
            throw new DomainException($"Cannot remove order item {orderItem} to order with status {Status}.");

        
        _items.Remove(orderItem);
    }
   
    #region Setters datetimes
    public void SetUpdatedAt()
    {
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SetCancelledAt()
    {
        CancelledAt = DateTime.UtcNow;
    }
    
    public void SetPaidAt()
    {
        PaidAt = DateTime.UtcNow;
    }
    
    public void SetShippedAt()
    {
        ShippedAt = DateTime.UtcNow;
    }
    
    public void SetDeliveredAt()
    {
        DeliveredAt = DateTime.UtcNow;
    }
    
    public void SetDiscount(decimal discount)
    {
        if(Status != OrderStatus.Pending)
            throw new DomainException($"Cannot set discount to order with status {Status}.");
                
        if(discount < 0)
            throw new DomainException("Discount cannot be negative",nameof(discount));
        
        Discount = discount;
    }
    #endregion
    
    #region Setters orderStatus
    public void SetShipped()
    {
        TransitionTo(OrderStatus.Shipped);
        SetShippedAt();
    }
    
    public void SetCancelled()
    {
        TransitionTo(OrderStatus.Cancelled);
        SetCancelledAt();
    }
    
    public void SetProcessing()
    {
        TransitionTo(OrderStatus.Processing);
    }
    
    public void SetDelivered()
    {
        TransitionTo(OrderStatus.Delivered);
        SetDeliveredAt();
    }
    
    public void SetReturned()
    {
        TransitionTo(OrderStatus.Returned);
    }
    #endregion
    
    #region Setters paymentStatus
    public void SetPaymentPending()
    {
        TransitionPaymentTo(PaymentStatus.Pending);
    }

    public void SetPaymentProcessing()
    {
        TransitionPaymentTo(PaymentStatus.Processing);
    }

    public void SetPaymentApproved()
    {
        TransitionPaymentTo(PaymentStatus.Approved);
        SetPaidAt();
    }

    public void SetPaymentRejected()
    {
        TransitionPaymentTo(PaymentStatus.Rejected);
    }

    public void SetPaymentRefunded()
    {
        TransitionPaymentTo(PaymentStatus.Refunded);
    }
    #endregion
    
    #region private methods transitionsValidation
    private static readonly Dictionary<OrderStatus, OrderStatus[]> _validTransitions = new()
    {
        [OrderStatus.Pending] = [OrderStatus.Processing, OrderStatus.Cancelled],
        [OrderStatus.Processing] = [OrderStatus.Shipped, OrderStatus.Cancelled],
        [OrderStatus.Shipped] = [OrderStatus.Delivered, OrderStatus.Returned],
        [OrderStatus.Delivered] = [OrderStatus.Returned],
    };

    private void TransitionTo(OrderStatus newStatus)
    {
        if (!_validTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
            throw new DomainException($"Invalid transition from {Status} to {newStatus}.", nameof(newStatus));

        Status = newStatus;
        SetUpdatedAt();
    }
    
    private static readonly Dictionary<PaymentStatus, PaymentStatus[]> _validPaymentTransitions = new()
    {
        [PaymentStatus.Pending] = [PaymentStatus.Processing, PaymentStatus.Rejected],
        [PaymentStatus.Processing] = [PaymentStatus.Approved, PaymentStatus.Rejected],
        [PaymentStatus.Approved] = [PaymentStatus.Refunded],
    };

    private void TransitionPaymentTo(PaymentStatus newStatus)
    {
        if (!_validPaymentTransitions.TryGetValue(PaymentStatus, out var allowed) || !allowed.Contains(newStatus))
            throw new DomainException($"Invalid payment transition from {PaymentStatus} to {newStatus}.");

        PaymentStatus = newStatus;
        SetUpdatedAt();
    }
    #endregion
}