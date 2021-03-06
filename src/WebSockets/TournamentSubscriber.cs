﻿using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Orleans.Tournament.Domain.Abstractions;
using Orleans.Tournament.Domain.Abstractions.Grains;
using Constants = Orleans.Tournament.Domain.Helpers;

namespace Orleans.Tournament.WebSockets
{
    [ImplicitStreamSubscription(Constants.TournamentNamespace)]
    public class TournamentSubscriber : SubscriberGrain
    {
        public TournamentSubscriber(ILogger<TournamentSubscriber> logger)
            : base(
                new StreamOptions(Constants.MemoryProvider, Constants.TournamentNamespace),
                new PrefixLogger(logger, "[Tournament][WebSocket]"))
        {
        }

        public override async Task<bool> HandleAsync(object evt, StreamSequenceToken token = null)
        {
            if (evt is ITraceable obj)
            {
                var streamToPublish = GetStreamProvider(Constants.MemoryProvider);
                var stream = streamToPublish.GetStream<object>(obj.InvokerUserId, Constants.WebSocketNamespace);
                await stream.OnNextAsync(new WebSocketMessage(evt.GetType().Name, evt));
            }

            return true;
        }
    }
}
