namespace Order.CustomExceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, string nameof) : base(message) { }
}