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

namespace IntegrationTests;

using System.Collections.Concurrent;
using HealthGateway.Common.Messaging;
using HealthGateway.GatewayApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

public class ServiceBusTests : ScenarioContextBase<Startup>
{
    private readonly Lazy<string> lazyQueueName;

    private string QueueName => this.lazyQueueName.Value;

    public ServiceBusTests(ITestOutputHelper output, WebAppFixture fixture) : base(output, fixture)
    {
        this.lazyQueueName = new Lazy<string>(() => this.Host.Services.GetRequiredService<IConfiguration>().GetValue("PhsaV2:ServiceBus:QueueName", string.Empty)!);
    }

    [Fact]
    public async Task SendNoSession_ReceiveNoSession()
    {
        using var cts = new CancellationTokenSource();
        var bus = this.Host.Services.GetRequiredService<IMessageBus>();
        var messages = Enumerable.Range(0, 2).Select(_ => new TestMessage()).ToArray();

        var responses = new ConcurrentBag<Message>();
        await bus.Subscribe(
            this.QueueName,
            async m => await Task.Run(() =>
            {
                this.Output.WriteLine("message {0}", m);
                responses.Add(m);
            }),
            async e => await Task.Run(() => this.Output.WriteLine("ERROR: {0}", e)),
            cts.Token);

        await bus.Send(this.QueueName, messages, cts.Token);

        cts.CancelAfter(1000 * 10);
        while (!cts.IsCancellationRequested && responses.IsEmpty) await Task.Delay(1000);

        responses.Count.ShouldBe(4);
        responses.ShouldAllBe(m => m is TestMessage);
    }

    [Fact]
    public async Task SendSession_ReceiveSpecificSession()
    {
        using var cts = new CancellationTokenSource();
        var bus = this.Host.Services.GetRequiredService<IMessageBus>();
        var s1Messages = Enumerable.Range(0, 2).Select(_ => new TestMessage("session1")).ToArray();
        var s2Messages = Enumerable.Range(0, 2).Select(_ => new TestMessage("session2")).ToArray();

        var responses = new ConcurrentBag<Message>();
        await bus.Subscribe(
            this.QueueName,
            "session1",
            async (m, s) => await Task.Run(() =>
            {
                this.Output.WriteLine("message {0}", m);
                responses.Add(m);
            }),
            async e => await Task.Run(() => this.Output.WriteLine("ERROR: {0}", e)),
            cts.Token);

        var sendTasks = new[] { bus.Send(this.QueueName, s1Messages, cts.Token), bus.Send(this.QueueName, s2Messages, cts.Token) };
        await Task.WhenAll(sendTasks);

        cts.CancelAfter(1000 * 10);
        while (!cts.IsCancellationRequested && responses.IsEmpty) await Task.Delay(1000);

        responses.Count.ShouldBe(2);
        var results = responses.Cast<TestMessage>();
        results.ShouldAllBe(m => m.SessionId == "session1");
    }

    [Fact]
    public async Task SendSession_ReceiveAllSessions()
    {
        using var cts = new CancellationTokenSource();
        var bus = this.Host.Services.GetRequiredService<IMessageBus>();
        var s1Messages = Enumerable.Range(0, 2).Select(_ => new TestMessage(_.ToString(), "session1")).ToArray();
        var s2Messages = Enumerable.Range(0, 2).Select(_ => new TestMessage(_.ToString(), "session2")).ToArray();

        var responses = new ConcurrentBag<Message>();
        await bus.Subscribe(
            this.QueueName,
            string.Empty,
            async (m, s) => await Task.Run(() =>
            {
                this.Output.WriteLine("message {0}", m);
                responses.Add(m);
            }),
            async e => await Task.Run(() => this.Output.WriteLine("ERROR: {0}", e)),
            cts.Token);

        var sendTasks = new[] { bus.Send(this.QueueName, s1Messages, cts.Token), bus.Send(this.QueueName, s2Messages, cts.Token) };
        await Task.WhenAll(sendTasks);

        cts.CancelAfter(1000 * 10);
        while (!cts.IsCancellationRequested && responses.Count < 4) await Task.Delay(1000);

        responses.Count.ShouldBe(4);
    }

    [Fact]
    public async Task SendSession1Fails_ReceiveOnlySession2()
    {
        using var cts = new CancellationTokenSource();
        var bus = this.Host.Services.GetRequiredService<IMessageBus>();
        //var s1Messages = Enumerable.Range(0, 2).Select(_ => new TestMessage("session1", true)).ToArray();
        var s1Messages = new[]
        {
            new TestMessage("1", "session1", true),
            new TestMessage("2", "session1", false)
        };
        var s2Messages = Enumerable.Range(0, 2).Select(_ => new TestMessage(_.ToString(), "session2")).ToArray();

        var responses = new ConcurrentBag<Message>();
        await bus.Subscribe(
            this.QueueName,
            string.Empty,
            async (m, s) => await Task.Run(() =>
            {
                this.Output.WriteLine("message {0}", m);
                var testMessage = (TestMessage)m;
                if (testMessage.ShouldError) throw new ApplicationException($"Message {testMessage.Id} is failing");
                responses.Add(m);
            }),
            async e => await Task.Run(() => this.Output.WriteLine("ERROR: {0}", e)),
            cts.Token);

        var sendTasks = new[] { bus.Send(this.QueueName, s1Messages, cts.Token), bus.Send(this.QueueName, s2Messages, cts.Token) };
        await Task.WhenAll(sendTasks);

        cts.CancelAfter(1000 * 30);
        while (!cts.IsCancellationRequested && responses.IsEmpty) await Task.Delay(5000);

        //responses.Count.ShouldBe(2);
        responses.ShouldNotBeEmpty();
        var results = responses.Cast<TestMessage>();
        results.ShouldAllBe(m => m.SessionId == "session2");
    }
}

public record TestMessage(string? Id = null, string? Hdid = null, bool ShouldError = false) : Message(Hdid)
{
    public string Id { get; init; } = Id ?? Guid.NewGuid().ToString().Substring(0, 6);
}
