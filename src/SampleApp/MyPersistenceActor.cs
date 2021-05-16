using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Persistence;
using Proto.Persistence.SnapshotStrategies;
using Proto.Persistence.Sqlite;
using Event = Proto.Persistence.Event;
using Snapshot = Proto.Persistence.Snapshot;

namespace SampleApp
{
    public class DtoPersistenceActor : IActor
    {
        private int _value = 0;

        public DtoPersistenceActor(ILogger<DtoPersistenceActor> logger,
                                   IProvider provider,
                                   string actorId)
        {
            Persistence = Persistence.WithEventSourcingAndSnapshotting(
                provider,
                provider,
                actorId,
                ApplyEvent,
                ApplySnapshot,
                new IntervalStrategy(10),
                () => QueueDto);
            Logger = logger;
        }

        private void ApplySnapshot(Snapshot snapshot)
        {
            if (snapshot.State is Collection<SampleDto> ss)
            {
                QueueDto = ss;
                Logger.LogInformation("Restore Snapshot : {0}", string.Join(",", QueueDto.Select(x => x.Number)));
            }
        }

        private void ApplyEvent(Event @event)
        {
            switch (@event.Data)
            {
                case SampleDto msg:
                    QueueDto.Add(msg);
                    Logger.LogInformation("Restore Queue : {0}", string.Join(",", QueueDto.Select(x => x.Number)));
                    break;
            }
        }

        public Persistence Persistence { get; }
        public ILogger<DtoPersistenceActor> Logger { get; }

        public Collection<SampleDto> QueueDto { get; private set; } = new Collection<SampleDto>();

        public Task ReceiveAsync(IContext context) => context.Message switch
        {
            Started => Persistence.RecoverStateAsync(),
            SampleDto msg => HandleAsync(msg),
            _ => throw new NotImplementedException()
        };

        private async Task HandleAsync(SampleDto msg) => await Persistence.PersistEventAsync(msg);
    }
}
