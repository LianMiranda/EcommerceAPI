using Order.CustomExceptions;
using Order.Enums;
using BuildingBlocks.Domain.Entities;
using Order.DomainEvents;

namespace Order.Entities;

public class Order : AggregateRoot
{
    public int Number { get; private set; }
    public Guid CustomerId { get; private set; }
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public OrderStatus Status { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public byte DiscountPercentage { get; private set; }
    public decimal Discount => Math.Round(Subtotal * (DiscountPercentage / 100m), 2, MidpointRounding.AwayFromZero);
    public decimal Subtotal => _items.Sum(i => i.TotalPrice);
    public decimal Total => Subtotal - Discount;
    public DateTime? PaidAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    private readonly List<Refund> _refunds = new();
    public IReadOnlyCollection<Refund> Refunds => _refunds.AsReadOnly();
    public decimal TotalRefunded => _refunds.Sum(r => r.Amount);
    public bool IsFullyRefunded => TotalRefunded >= Total;
    public bool IsPartiallyRefunded => TotalRefunded > 0 && !IsFullyRefunded;

    private Order() {}
    
    public static Order Create(Guid customerId, byte discountPercentage, List<OrderItem> items)
    {
        if(customerId == Guid.Empty)
            throw new DomainException("Cannot be empty",nameof(customerId));
        
        if(discountPercentage > 100)
            throw new DomainException("Discount Percentage cannot exceed 100", nameof(discountPercentage));
        
        if(items is null || items.Count < 1)
            throw new DomainException("Items cannot be empty", nameof(items));
        
        var order = new Order
        {
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            DiscountPercentage = discountPercentage,
            PaidAt = null,
            ShippedAt = null,
            DeliveredAt = null,
            CancelledAt = null
        };
        
        foreach (var item in items)
            order.AddItem(item);
        
        order.RaiseDomainEvent(new OrderCreatedDomainEvent(order.Id, order.CustomerId, order.Total));
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
    
    public void RegisterRefund(decimal amount, string reason, Guid? orderItemId = null)
    {
        if (PaymentStatus != PaymentStatus.Approved)
            throw new DomainException($"Cannot refund payment with status {PaymentStatus}.");

        if (TotalRefunded + amount > Total)
            throw new DomainException("Refund amount exceeds total paid.", nameof(amount));

        if (orderItemId is not null && !_items.Any(i => i.Id == orderItemId))
            throw new DomainException("Order item does not belong to this order.", nameof(orderItemId));

        var refund = Refund.Create(Id, orderItemId, amount, reason);
        _refunds.Add(refund);
    
        if (IsFullyRefunded)
            SetPaymentRefunded();
    }
   
    #region Setters datetimes
    
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
    
    public void SetDiscount(byte discountPercentage)
    {
        if(Status != OrderStatus.Pending)
            throw new DomainException($"Cannot set discount to order with status {Status}.");
                
        if(discountPercentage > 0)
            throw new DomainException("Discount Percentage cannot exceed 100.",nameof(discountPercentage));
        
        DiscountPercentage = discountPercentage;
    }
    #endregion
    
    #region Setters orderStatus
    public void SetShipped()
    {
        TransitionTo(OrderStatus.Shipped);
        SetShippedAt();
        RaiseDomainEvent(new OrderShippedDomainEvent(Id, ShippedAt!.Value));
    }
    
    public void SetCancelled()
    {
        TransitionTo(OrderStatus.Cancelled);
        SetCancelledAt();
        RaiseDomainEvent(new OrderCancelledDomainEvent(Id, CancelledAt!.Value));

    }
    
    public void SetProcessing()
    {
        TransitionTo(OrderStatus.Processing);
    }
    
    public void SetDelivered()
    {
        TransitionTo(OrderStatus.Delivered);
        SetDeliveredAt();
        RaiseDomainEvent(new OrderDeliveredDomainEvent(Id, DeliveredAt!.Value));
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

    private void SetPaymentRefunded()
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