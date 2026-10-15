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
using System.Diagnostics.CodeAnalysis;
using System.Transactions;
using Azure.Messaging.ServiceBus;
using Hangfire;
using HealthGateway.Common.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CA1001 // Types that own disposable fields should be disposable

[IntegrationTest]
public class MessagingTests : ScenarioContextBase<GatewayApi.Program>
{
    private readonly CancellationTokenSource cts = new();
    private readonly ConcurrentBag<Exception> receiveErrors = [];
    private readonly WebAppFixture messagingFixture;
    private readonly string[] sessionIds = [Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N")];
    private bool disposed;
    private IMessageReceiver receiver = null!;

    // Keep the collection's sequential execution, but never share its database with API tests.
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The test owns the fixture and disposes it in DisposeAsync, including initialization failures.")]
    public MessagingTests(ITestOutputHelper output) : this(output, new WebAppFixture())
    {
    }

    private MessagingTests(ITestOutputHelper output, WebAppFixture fixture) : base(output, TestConfiguration.WebClientConfigSection, fixture)
    {
        this.messagingFixture = fixture;
    }

    protected override IEnumerable<KeyValuePair<string, string?>> ConfigurationSettings =>
    [
        new("PhsaV2:ServiceBus:UseOutbox", "true"),
    ];

    protected override void ConfigureServices(IServiceCollection services)
    {
        // Scope the real Azure receiver at the broker, rather than consuming and ignoring other tests' messages.
        services.AddSingleton<IAzureClientFactory<ServiceBusClient>>(sp => new SessionScopedServiceBusClientFactory(
            sp.GetRequiredService<IConfiguration>()["PhsaV2:ServiceBus:ConnectionString"]
                ?? throw new InvalidOperationException("Integration tests require a Service Bus connection string."),
            this.sessionIds));
        services.AddHangfireServer(
            opts =>
            {
                opts.Queues = [AzureServiceBusSettings.OutboxQueueName];
                opts.IsLightweightServer = true;
            });
    }

    public override async ValueTask InitializeAsync()
    {
        try
        {
            this.Output.WriteLine("Starting isolated messaging database and Redis containers.");
            await this.messagingFixture.InitializeAsync();
            this.Output.WriteLine("Starting the messaging test API host.");
            await base.InitializeAsync();

            this.receiver = this.TestServices.GetRequiredService<IMessageReceiver>();
        }
        catch
        {
            await this.DisposeAsync();
            throw;
        }
    }

    public override async ValueTask DisposeAsync()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;
        try
        {
            // Stop the host and its processor before disposing the database.
            await base.DisposeAsync();
        }
        finally
        {
            this.cts.Dispose();
            await this.messagingFixture.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }

    private async Task SendMessages(IEnumerable<IEnumerable<TestMessage>> messageGroups, CancellationToken ct)
    {
        IMessageSender sender = this.TestServices.GetRequiredService<IMessageSender>();

        foreach (IEnumerable<TestMessage> messages in messageGroups)
        {
            await sender.SendAsync(messages.Select(m => new MessageEnvelope(m, m.SessionId)), ct);
        }

        // WIP - Yossi still developing approach
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
        ConcurrentBag<MessageEnvelope> receivedMessages = [];
        await this.receiver.SubscribeAsync(
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
            e =>
            {
                this.receiveErrors.Add(e);
                this.Output.WriteLine("RECEIVE ERROR: {0}", e);
                return Task.CompletedTask;
            },
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

    private int CountSessionErrors(string sessionId)
    {
        return this.receiveErrors.Count(e => e is InvalidOperationException && e.Message.Contains(sessionId, StringComparison.Ordinal));
    }

    [Fact]
    public async Task SendSession_ReceiveAllSessions()
    {
        string s1 = this.sessionIds[0];
        string s2 = this.sessionIds[1];
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
        string s1 = this.sessionIds[0];
        string s2 = this.sessionIds[1];
        TestMessage[] s1Messages =
        {
            new("1", s1, true),
            new("2", s1),
        };
        TestMessage[] s2Messages = Enumerable.Range(0, 2).Select(n => new TestMessage(n.ToString(), s2)).ToArray();

        ConcurrentBag<MessageEnvelope> responses = await this.ReceiveMessages(this.cts.Token);
        await this.SendMessages(new[] { s1Messages, s2Messages }, this.cts.Token);
        // Wait for both failed-session messages, not just the successful session.
        await this.WaitForMessages(() => responses.Count >= 2 && this.CountSessionErrors(s1) >= 2);

        this.CountSessionErrors(s1).ShouldBe(2);
        responses.Count.ShouldBe(2);
        responses.ShouldAllBe(m => m.SessionId == s2);
    }

    [Fact]
    public async Task SendMessagesInTransaction_Rollback_ReceiveNoMessages()
    {
        string sessionId = this.sessionIds[0];
        TestMessage[] messages = Enumerable.Range(0, 2).Select(n => new TestMessage(n.ToString(), sessionId)).ToArray();

        ConcurrentBag<MessageEnvelope> responses = await this.ReceiveMessages(this.cts.Token);

        using (TransactionScope _ = new(TransactionScopeAsyncFlowOption.Enabled))
        {
            await this.SendMessages(new[] { messages }, this.cts.Token);
        }

        await this.Assert(async ctx =>
        {
            bool hasRolledBackMessages = await ctx.Outbox.AnyAsync(item => item.Metadata.SessionId == sessionId);
            hasRolledBackMessages.ShouldBeFalse();
        });

        // A committed control message proves the dispatcher and receiver are working.
        string controlSessionId = this.sessionIds[1];
        await this.SendMessages(new[] { new[] { new TestMessage("control", controlSessionId) } }, this.cts.Token);
        await this.WaitForMessages(() => responses.Any(m => m.SessionId == controlSessionId));
        responses.Count(m => m.SessionId == controlSessionId).ShouldBe(1);

        await this.WaitForMessages(() => responses.Any(m => m.SessionId == sessionId));
        responses.Count(m => m.SessionId == sessionId).ShouldBe(0);
    }

#pragma warning restore CA1063 // Implement IDisposable Correctly
}

public record TestMessage(string? Id = null, string? SessionId = null, bool ShouldError = false) : MessageBase
{
    public string Id { get; init; } = Id ?? Guid.NewGuid().ToString()[..6];
}
