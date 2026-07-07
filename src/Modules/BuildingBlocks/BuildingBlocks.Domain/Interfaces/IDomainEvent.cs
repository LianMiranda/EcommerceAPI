namespace BuildingBlocks.Domain.Interfaces;

public interface IDomainEvent
{
    Guid EventId => Guid.CreateVersion7();
    DateTime OccurredOn { get; }
}