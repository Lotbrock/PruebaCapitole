using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PruebaCapitole.Domain.Common;
using PruebaCapitole.Service.Abstractions;

namespace PruebaCapitole.Infrastructure.Data.Interceptors
{
    /// <summary>
    /// Publishes the domain events of the aggregates right BEFORE the save, so the handlers
    /// (which write to the Outbox) end up in the same transaction as the business change.
    /// Handlers never dispatch events by hand.
    /// </summary>
    public sealed class DispatchDomainEventsInterceptor(IPublisher publisher) : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
                await DispatchAsync(eventData.Context, cancellationToken);

            return result;
        }

        private async Task DispatchAsync(DbContext context, CancellationToken ct)
        {
            var aggregates = context.ChangeTracker.Entries<IHasDomainEvents>()
                .Select(e => e.Entity)
                .Where(a => a.DomainEvents.Count > 0)
                .ToList();

            var events = aggregates.SelectMany(a => a.DomainEvents).ToList();
            aggregates.ForEach(a => a.ClearDomainEvents());

            foreach (var domainEvent in events)
            {
                var type = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
                await publisher.Publish(Activator.CreateInstance(type, domainEvent)!, ct);
            }
        }
    }
}
