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

namespace HealthGateway.IntegrationTests;

using System.Collections.Concurrent;
using System.Transactions;
using Hangfire;
using Hangfire.PostgreSql;
using HealthGateway.Common.Messaging;
using HealthGateway.Common.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit.Abstractions;

#pragma warning disable CA1063 // Implement IDisposable Correctly

public class HangfireMessageSenderTests : ScenarioContextBase<GatewayApi.Startup>, IDisposable
{
    private readonly CancellationTokenSource cts;
    private IMessageSender sender = null!;
    private IMessageReceiver receiver = null!;
    private BackgroundJobServer hangfireBackgroundJobServer = null!;

    public HangfireMessageSenderTests(ITestOutputHelper output, WebAppFixture fixture) : base(output, fixture)
    {
        this.cts = new CancellationTokenSource();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        this.sender = this.Host.Services.GetRequiredService<IMessageSender>();
        this.receiver = this.Host.Services.GetRequiredService<IMessageReceiver>();
#pragma warning disable CA2326 // Do not use TypeNameHandling values other than None
        GlobalConfiguration.Configuration
            .UseSerializerSettings(new Newtonsoft.Json.JsonSerializerSettings { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All })
            .UsePostgreSqlStorage(this.Host.Services.GetRequiredService<IConfiguration>().GetConnectionString("GatewayConnection"));
#pragma warning restore CA2326 // Do not use TypeNameHandling values other than None
        this.hangfireBackgroundJobServer = new BackgroundJobServer(new BackgroundJobServerOptions
        {
            Queues = new[] { AzureServiceBusSettings.OutboxQueueName },
        });
    }

    private async Task SendMessages(IEnumerable<TestMessage>[] messages, CancellationToken ct)
    {
        var sendTasks = messages.Select(messages => this.sender.SendAsync(messages.Select(m => new MessageEnvelope(m, m.SessionId)), ct));
        await Task.WhenAll(sendTasks);
    }

    private async Task<ConcurrentBag<MessageEnvelope>> ReceiveMessages(CancellationToken ct)
    {
        var receivedMessages = new ConcurrentBag<MessageEnvelope>();
        await this.receiver.Subscribe(
            async (sessionId, messages) => await Task.Run(() =>
            {
                foreach (var m in messages)
                {
                    if (m.Content is TestMessage tm && tm.ShouldError)
                    {
                        throw new InvalidOperationException($"Test {tm.SessionId}:{tm.Id} message failed");
                    }
                    receivedMessages.Add(m);
                }
                return true;
            }, ct),
            async e => await Task.Run(() => this.Output.WriteLine("RECEIVE ERROR: {0}", e)), ct);

        return receivedMessages;
    }

    private async Task<bool> WaitForMessages(Func<bool> stopCondition, TimeSpan? timeout = null)
    {
        cts.CancelAfter(timeout ?? TimeSpan.FromSeconds(10));
        while (!cts.IsCancellationRequested)
        {
            if (stopCondition()) return true;
            await Task.Delay(1000);
        }
        return false;
    }

    private static string GenerateSessionId() => Guid.NewGuid().ToString().Substring(0, 4);

    [Fact]
    public async Task SendSession_ReceiveAllSessions()
    {
        var s1 = GenerateSessionId();
        var s2 = GenerateSessionId();
        var s1Messages = Enumerable.Range(0, 2).Select(_ => new TestMessage(_.ToString(), s1)).ToArray();
        var s2Messages = Enumerable.Range(0, 2).Select(_ => new TestMessage(_.ToString(), s2)).ToArray();

        var responses = await ReceiveMessages(cts.Token);
        await SendMessages(new[] { s1Messages, s2Messages }, this.cts.Token);
        await WaitForMessages(() => !responses.IsEmpty);

        responses.Count.ShouldBe(4);
        responses.Count(m => m.SessionId == s1).ShouldBe(2);
        responses.Count(m => m.SessionId == s2).ShouldBe(2);
    }

    [Fact]
    public async Task SendSession1Fails_ReceiveOnlySession2()
    {
        var s1 = GenerateSessionId();
        var s2 = GenerateSessionId();
        var s1Messages = new[]
        {
            new TestMessage("1", s1, true),
            new TestMessage("2", s1, false)
        };
        var s2Messages = Enumerable.Range(0, 2).Select(_ => new TestMessage(_.ToString(), s2)).ToArray();

        var responses = await ReceiveMessages(cts.Token);
        await SendMessages(new[] { s1Messages, s2Messages }, this.cts.Token);
        await WaitForMessages(() => !responses.IsEmpty);

        responses.Count.ShouldBe(2);
        responses.ShouldAllBe(m => m.SessionId == s2);
    }

    [Fact]
    public async Task SendMessagesInTransaction_Rollback_ReveiveNoMessages()
    {
        var sessionId = GenerateSessionId();
        var messages = Enumerable.Range(0, 2).Select(_ => new TestMessage(_.ToString(), sessionId)).ToArray();

        var responses = await ReceiveMessages(cts.Token);

        using (var tx = new TransactionScope())
        {
            await SendMessages(new[] { messages }, this.cts.Token);
        }

        await WaitForMessages(() => !responses.IsEmpty);

        responses.Count.ShouldBe(0);
    }

    [Fact]
    public void CanSerializeMessageEnvelopeArray()
    {
        var sessionId = GenerateSessionId();
        var messages = Enumerable.Range(0, 2).Select(_ => new TestMessage(_.ToString(), sessionId));
        var envelopes = messages.Select(m => new MessageEnvelope(m, m.SessionId)).ToArray();
        var serialized = envelopes.Serialize();

        var deserializedMessage = serialized.Deserialize<IEnumerable<MessageEnvelope>>();

        deserializedMessage.ShouldNotBeNull().Count().ShouldBe(envelopes.Length);
    }

    public void Dispose()
    {
        this.cts.Dispose();
        this.hangfireBackgroundJobServer.Dispose();
        GC.SuppressFinalize(this);
    }

#pragma warning restore CA1063 // Implement IDisposable Correctly
}

public record TestMessage(string? Id = null, string? SessionId = null, bool ShouldError = false) : MessageBase()
{
    public string Id { get; init; } = Id ?? Guid.NewGuid().ToString().Substring(0, 6);
}
