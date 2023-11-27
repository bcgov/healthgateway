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

namespace HealthGateway.IntegrationTests.Infrastructure;

using System.Collections.Concurrent;
using System.Transactions;
using Hangfire;
using Hangfire.PostgreSql;
using HealthGateway.Common.Messaging;
using HealthGateway.Common.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using Xunit.Categories;

#pragma warning disable CA1001 // Types that own disposable fields should be disposable

[IntegrationTest]
public class MessagingTests : ScenarioContextBase<GatewayApi.Program>
{
    private readonly CancellationTokenSource cts;
    private IMessageReceiver receiver = null!;

    public MessagingTests(ITestOutputHelper output, WebAppFixture fixture) : base(output, TestConfiguration.WebClientConfigSection, fixture)
    {
        this.cts = new CancellationTokenSource();
        fixture.Services.AddHangfireServer(
            opts =>
            {
                opts.Queues = new[] { AzureServiceBusSettings.OutboxQueueName };
                opts.IsLightweightServer = true;
            });
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        this.receiver = this.TestServices.GetRequiredService<IMessageReceiver>();

        GlobalConfiguration.Configuration.UsePostgreSqlStorage(
            c => c.UseNpgsqlConnection(
                this.TestServices.GetRequiredService<IConfiguration>().GetConnectionString("GatewayConnection")));
    }

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();
        this.cts.Dispose();
    }

    private async Task SendMessages(IEnumerable<IEnumerable<TestMessage>> messageGroups, CancellationToken ct)
    {
        IMessageSender sender = this.TestServices.GetRequiredService<IMessageSender>();

        foreach (var messages in messageGroups)
        {
            await sender.SendAsync(messages.Select(m => new MessageEnvelope(m, m.SessionId)), ct);
        }

        //await Parallel.ForEachAsync(
        //    messageGroups,
        //    ct,
        //    async (messages, ct1) =>
        //    {
        //        this.Output.WriteLine("sending messages");
        //        //using IServiceScope scope = this.TestServices.CreateScope();
        //        //IMessageSender sender = scope.ServiceProvider.GetRequiredService<IMessageSender>();
        //        await sender.SendAsync(messages.Select(m => new MessageEnvelope(m, m.SessionId)), ct1);
        //    });
    }

    private async Task<ConcurrentBag<MessageEnvelope>> ReceiveMessages(CancellationToken ct)
    {
        ConcurrentBag<MessageEnvelope> receivedMessages = new();
        await this.receiver.Subscribe(
            async (_, messages) => await Task.Run(
                () =>
                {
                    foreach (MessageEnvelope m in messages)
                    {
                        if (m.Content is TestMessage tm && tm.ShouldError)
                        {
                            throw new InvalidOperationException($"Test {tm.SessionId}:{tm.Id} message failed");
                        }

                        receivedMessages.Add(m);
                    }

                    return true;
                },
                ct),
            async e => await Task.Run(() => this.Output.WriteLine("RECEIVE ERROR: {0}", e)),
            ct);

        return receivedMessages;
    }

    private async Task WaitForMessages(Func<bool> stopCondition, TimeSpan? timeout = null)
    {
        this.cts.CancelAfter(timeout ?? TimeSpan.FromSeconds(10));
        while (!this.cts.IsCancellationRequested)
        {
            if (stopCondition())
            {
                return;
            }

            await Task.Delay(1000);
        }
    }

    private static string GenerateSessionId()
    {
        return Guid.NewGuid().ToString()[..4];
    }

    [Fact]
    public async Task SendSession_ReceiveAllSessions()
    {
        string s1 = GenerateSessionId();
        string s2 = GenerateSessionId();
        TestMessage[] s1Messages = Enumerable.Range(0, 2).Select(n => new TestMessage(n.ToString(), s1)).ToArray();
        TestMessage[] s2Messages = Enumerable.Range(0, 2).Select(n => new TestMessage(n.ToString(), s2)).ToArray();

        ConcurrentBag<MessageEnvelope> responses = await this.ReceiveMessages(this.cts.Token);
        await this.SendMessages(new[] { s1Messages, s2Messages }, this.cts.Token);
        await this.WaitForMessages(() => responses.Count >= 4);

        responses.Count.ShouldBe(4);
        responses.Count(m => m.SessionId == s1).ShouldBe(2);
        responses.Count(m => m.SessionId == s2).ShouldBe(2);
    }

    [Fact]
    public async Task SendSession1Fails_ReceiveOnlySession2()
    {
        string s1 = GenerateSessionId();
        string s2 = GenerateSessionId();
        TestMessage[] s1Messages =
        {
            new("1", s1, true),
            new("2", s1),
        };
        TestMessage[] s2Messages = Enumerable.Range(0, 2).Select(n => new TestMessage(n.ToString(), s2)).ToArray();

        ConcurrentBag<MessageEnvelope> responses = await this.ReceiveMessages(this.cts.Token);
        await this.SendMessages(new[] { s1Messages, s2Messages }, this.cts.Token);
        await this.WaitForMessages(() => responses.Count >= 2);

        responses.Count.ShouldBe(2);
        responses.ShouldAllBe(m => m.SessionId == s2);
    }

    [Fact]
    public async Task SendMessagesInTransaction_Rollback_ReceiveNoMessages()
    {
        string sessionId = GenerateSessionId();
        TestMessage[] messages = Enumerable.Range(0, 2).Select(n => new TestMessage(n.ToString(), sessionId)).ToArray();

        ConcurrentBag<MessageEnvelope> responses = await this.ReceiveMessages(this.cts.Token);

        using (TransactionScope _ = new(TransactionScopeAsyncFlowOption.Enabled))
        {
            await this.SendMessages(new[] { messages }, this.cts.Token);
        }

        await this.WaitForMessages(() => !responses.IsEmpty);

        responses.Count.ShouldBe(0);
    }

    [Fact]
    public void CanSerializeMessageEnvelopeArray()
    {
        string sessionId = GenerateSessionId();
        IEnumerable<TestMessage> messages = Enumerable.Range(0, 2).Select(n => new TestMessage(n.ToString(), sessionId));
        MessageEnvelope[] envelopes = messages.Select(m => new MessageEnvelope(m, m.SessionId)).ToArray();
        byte[] serialized = envelopes.Serialize();

        IEnumerable<MessageEnvelope>? deserializedMessage = serialized.Deserialize<IEnumerable<MessageEnvelope>>();

        deserializedMessage.ShouldNotBeNull().Count().ShouldBe(envelopes.Length);
    }

#pragma warning restore CA1063 // Implement IDisposable Correctly
}

public record TestMessage(string? Id = null, string? SessionId = null, bool ShouldError = false) : MessageBase
{
    public string Id { get; init; } = Id ?? Guid.NewGuid().ToString()[..6];
}
