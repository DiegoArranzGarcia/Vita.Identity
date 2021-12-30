using System;
using Vita.Core.Domain.Events;
using Vita.Identity.Domain.Aggregates.Users;

namespace Vita.Identity.Application.Commands.IntegrationEvents
{
    public class CreatedUserIntegrationEvent : IntegrationEvent
    {
        private const int CreatedUserEventVersion = 1;

        public Guid EntityId { get; private set; }
        public string Entity => nameof(User);

        public CreatedUserIntegrationEvent(Guid entityId) : base(CreatedUserEventVersion, nameof(CreatedUserIntegrationEvent))
        {
            EntityId = entityId;
        }
    }
}
