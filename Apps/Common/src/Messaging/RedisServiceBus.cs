// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------

namespace HealthGateway.Common.Messaging;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

internal class RedisServiceBus : IMessageBus
{
    private readonly IConnectionMultiplexer connectionMultiplexer;
    private readonly ConcurrentDictionary<string, RedisChannel> channels = new ConcurrentDictionary<string, RedisChannel>();

    public RedisServiceBus(IConnectionMultiplexer connectionMultiplexer)
    {
        this.connectionMultiplexer = connectionMultiplexer;
    }

    /// <inheritdoc/>
    public async Task Send(string queue, IEnumerable<Message> messages, CancellationToken ct)
    {
        var channel = this.GetChannel(queue);
        foreach (RedisValue message in messages.Select(RedisValue.Unbox))
        {
            await this.connectionMultiplexer.GetDatabase().PublishAsync(channel, message);
        }
    }

    /// <inheritdoc/>
    public async Task Subscribe(string queue, Func<Message, Task> messageHandler, Func<Exception, Task> errorHandler, CancellationToken ct)
    {
        var channel = this.GetChannel(queue);
        var subscription = await this.connectionMultiplexer.GetSubscriber().SubscribeAsync(channel);
        subscription.OnMessage(async m =>
        {
            try
            {
                await messageHandler((Message)m.Message.Box()!);
            }
            catch (Exception e)
            {
                await errorHandler(e);
            }
        });
    }

    /// <inheritdoc/>
    public async Task Subscribe(string queue, string sessionId, Func<Message, string, Task> messageHandler, Func<Exception, Task> errorHandler, CancellationToken ct)
    {
        var channel = this.GetSessionChannel(queue, sessionId);
        var subscription = await this.connectionMultiplexer.GetSubscriber().SubscribeAsync(channel);
        subscription.OnMessage(async m =>
        {
            try
            {
                await messageHandler((Message)m.Message.Box()!, sessionId);
            }
            catch (Exception e)
            {
                await errorHandler(e);
            }
        });
    }

    private RedisChannel GetChannel(string queue) => this.channels.GetOrAdd(queue, q => new RedisChannel(q, RedisChannel.PatternMode.Literal));

    private RedisChannel GetSessionChannel(string queue, string sessionId) =>
        this.channels.GetOrAdd(queue, q => new RedisChannel($"{q}.{sessionId}", RedisChannel.PatternMode.Literal));
}
