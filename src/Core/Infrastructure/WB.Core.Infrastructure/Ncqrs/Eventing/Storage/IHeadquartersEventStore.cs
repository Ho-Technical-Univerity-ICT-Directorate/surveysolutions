﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ncqrs.Eventing.Storage
{
    public class RawEvent
    {
        public Guid Id { get; set; }
        public Guid EventSourceId { get; set; }
        public string Origin { get; set; }
        public int EventSequence { get; set; }
        public DateTime TimeStamp { get; set; }
        public long GlobalSequence { get; set; }
        public string EventType { get; set; }
        public string Value { get; set; }
    }

    public interface IHeadquartersEventStore : IEventStore
    {
        int CountOfAllEvents();

        bool HasEventsAfterSpecifiedSequenceWithAnyOfSpecifiedTypes(long sequence, Guid eventSourceId,
            params string[] typeNames);

        int? GetMaxEventSequenceWithAnyOfSpecifiedTypes(Guid eventSourceId, params string[] typeNames);

        Task<EventsFeedPage> GetEventsFeedAsync(long startWithGlobalSequence, int pageSize);
        IEnumerable<RawEvent> GetRawEventsFeed(long startWithGlobalSequence, int pageSize);
        Task<long> GetMaximumGlobalSequence();
    }
}