﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.EventHandlers;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal abstract class AbstractDenormalizer<TEntity> : IFunctionalEventHandler where TEntity : StateBase, new()
    {
        public void Handle(IEnumerable<IPublishableEvent> publishableEvents, Guid eventSourceId)
        {
            if (publishableEvents.Any(this.Handles))
            {
                var state = new TEntity {Id = eventSourceId};

                foreach (var publishableEvent in publishableEvents)
                {
                    if (!this.Handles(publishableEvent))
                        continue;

                    var eventType = typeof(IPublishedEvent<>).MakeGenericType(publishableEvent.Payload.GetType());

                    this.GetType().GetTypeInfo().GetMethod("Update", new[] {typeof(TEntity), eventType})
                        .Invoke(this, new object[] {state, this.CreatePublishedEvent(publishableEvent)});
                }

                this.SaveState(state);
            }
        }

        protected abstract void SaveState(TEntity state);

        protected PublishedEvent CreatePublishedEvent(IUncommittedEvent evt)
        {
            var publishedEventClosedType = typeof(PublishedEvent<>).MakeGenericType(evt.Payload.GetType());
            return (PublishedEvent)Activator.CreateInstance(publishedEventClosedType, evt);
        }

        protected virtual bool Handles(IUncommittedEvent evt)
            => typeof(IUpdateOrRemoveHandler<,>).MakeGenericType(typeof(TEntity), evt.Payload.GetType()).GetTypeInfo().IsInstanceOfType(this);

        public void RegisterHandlersInOldFashionNcqrsBus(InProcessEventBus oldEventBus)
        {
            //no need in current implementation
        }
    }
}