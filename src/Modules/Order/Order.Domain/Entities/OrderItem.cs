using Order.CustomExceptions;

namespace Order.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public decimal Discount { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice => (Quantity * UnitPrice) - Discount;

    public OrderItem(Guid productId, string productName, int quantity, decimal discount, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0)
            throw new DomainException("Unit price cannot be negative.", nameof(unitPrice));
        if (discount < 0)
            throw new DomainException("Discount cannot be negative.", nameof(discount));
        
        Id = Guid.CreateVersion7();
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        Discount = discount;
        UnitPrice = unitPrice;
    }

    public void SetDiscount(decimal discount)
    {
        if(discount < 0)
            throw new DomainException("Discount cannot be negative",nameof(discount));
        
        Discount = discount;
    }
}