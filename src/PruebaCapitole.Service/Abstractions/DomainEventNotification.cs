using MediatR;
using PruebaCapitole.Domain.Common;

namespace PruebaCapitole.Service.Abstractions
{
    /// <summary>
    /// Wraps a domain event so it can travel through MediatR without
    /// making the Domain depend on MediatR.
    /// </summary>
    public sealed record DomainEventNotification<TEvent>(TEvent DomainEvent) : INotification
        where TEvent : IDomainEvent;
}
