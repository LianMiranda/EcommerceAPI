using Order.CustomExceptions;

namespace Order.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public byte DiscountPercentage { get; private set; } 
    public decimal UnitPrice { get; private set; }
    public decimal Subtotal => Quantity * UnitPrice;
    public decimal Discount => Math.Round(Subtotal * (DiscountPercentage / 100m), 2, MidpointRounding.AwayFromZero);
    public decimal TotalPrice => Subtotal - Discount;

    private OrderItem() {}
    public static OrderItem Create(Guid productId, string productName, int quantity, byte discountPercentage, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0)
            throw new DomainException("Unit price cannot be negative.", nameof(unitPrice));
        if (discountPercentage < 0)
            throw new DomainException("Discount Percentage cannot be negative.", nameof(discountPercentage));
        if (string.IsNullOrEmpty(productName))
            throw new DomainException("Product name cannot be empty", nameof(productName));
        if(productId == Guid.Empty)
            throw new DomainException("Product ID cannot be empty", nameof(productId));
        
        return new OrderItem()
        {
            Id = Guid.CreateVersion7(),
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            DiscountPercentage = discountPercentage,
            UnitPrice = unitPrice
        };
    }

    public void SetDiscountPercentage(byte discountPercentage)
    {
        if(discountPercentage < 0)
            throw new DomainException("Discount cannot be negative",nameof(discountPercentage));
        
        DiscountPercentage = discountPercentage;
    }
}