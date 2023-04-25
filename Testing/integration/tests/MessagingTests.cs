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
using Hangfire;
using Hangfire.PostgreSql;
using HealthGateway.Common.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit.Abstractions;

#pragma warning disable CA1063 // Implement IDisposable Correctly

public class MessagingTests : ScenarioContextBase<GatewayApi.Startup>, IDisposable
{
    private readonly CancellationTokenSource cts;
    private IMessageSender sender = null!;
    private IMessageReceiver receiver = null!;
    private BackgroundJobServer hangfireBackgroundJobServer = null!;

    public MessagingTests(ITestOutputHelper output, WebAppFixture fixture) : base(output, fixture)
    {
        this.cts = new CancellationTokenSource();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        this.sender = this.Host.Services.GetRequiredService<IMessageSender>();
        this.receiver = this.Host.Services.GetRequiredService<IMessageReceiver>();

        GlobalConfiguration.Configuration.UsePostgreSqlStorage(this.Host.Services.GetRequiredService<IConfiguration>().GetConnectionString("GatewayConnection"));
        this.hangfireBackgroundJobServer = new BackgroundJobServer(new BackgroundJobServerOptions
        {
            Queues = new[] { HangFireOutboxDispatcher.OutboxQueueName },
        });
    }

    private async Task SendMessages(IEnumerable<MessageBase>[] messages, CancellationToken ct)
    {
        var sendTasks = messages.Select(m => this.sender.SendAsync(m, ct));
        await Task.WhenAll(sendTasks);
    }

    private async Task<ConcurrentBag<MessageBase>> ReceiveMessages(CancellationToken ct)
    {
        var receivedMessages = new ConcurrentBag<MessageBase>();
        await this.receiver.Subscribe(
            async (sessionId, messages) => await Task.Run(() =>
            {
                foreach (var m in messages)
                {
                    if (m is TestMessage tm && tm.ShouldError)
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
        var results = responses.Cast<TestMessage>();
        results.ShouldAllBe(m => m.SessionId == s2);
    }

    public void Dispose()
    {
        this.cts.Dispose();
        this.hangfireBackgroundJobServer.Dispose();
        GC.SuppressFinalize(this);
    }

#pragma warning restore CA1063 // Implement IDisposable Correctly
}

public record TestMessage(string? Id = null, string? SessionId = null, bool ShouldError = false) : MessageBase(SessionId)
{
    public string Id { get; init; } = Id ?? Guid.NewGuid().ToString().Substring(0, 6);
}
