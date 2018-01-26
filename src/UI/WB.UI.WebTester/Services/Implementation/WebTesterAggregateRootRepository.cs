﻿using System;
using System.Web.WebPages;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Infrastructure.Native.Storage;

namespace WB.UI.WebTester.Services.Implementation
{
    public class WebTesterAggregateRootRepository : EventSourcedAggregateRootRepositoryWithWebCache
    {
        private readonly IEvictionObserver notify;

        public WebTesterAggregateRootRepository(
            IEventStore eventStore,
            ISnapshotStore snapshotStore,
            IDomainRepository repository,
            IAggregateLock aggregateLock,
            IEvictionObserver notify,
            IEvictionObservable evictionNotification) : base(eventStore, snapshotStore, repository, aggregateLock)
        {
            this.notify = notify;
            Expiration = TimeSpan.FromMinutes(ConfigurationSource.Configuration["Cache.Expiration"].AsInt(10));
            evictionNotification.Subscribe(this.Evict);
        }

        const string CachePrefix = "cache:";

        protected override string Key(Guid id) => CachePrefix + id;

        protected override TimeSpan Expiration { get; }

        protected override void CacheItemRemoved(string key)
        {
            var interviewId = Guid.Parse(key.Substring(CachePrefix.Length));
            notify.Evict(interviewId);
            base.CacheItemRemoved(key);
        }
    }
}