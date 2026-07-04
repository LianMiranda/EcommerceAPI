namespace Order.Enums;

public enum OrderStatus
{
    Pending = 0,          // Pedido criado, aguardando pagamento
    Processing = 1,       // Separando/preparando o pedido
    Shipped = 2,          // Enviado para transportadora
    Delivered = 3,        // Entregue ao cliente
    Cancelled = 4,        // Cancelado
    Returned = 5          // Produto devolvido após entrega
}