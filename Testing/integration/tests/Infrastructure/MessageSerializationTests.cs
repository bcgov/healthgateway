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

using HealthGateway.Common.Messaging;
using HealthGateway.Common.Utils;

[IntegrationTest]
public class MessageSerializationTests
{
    [Fact]
    public void CanSerializeMessageEnvelopeArray()
    {
        string sessionId = Guid.NewGuid().ToString("N");
        IEnumerable<TestMessage> messages = Enumerable.Range(0, 2).Select(n => new TestMessage(n.ToString(), sessionId));
        MessageEnvelope[] envelopes = messages.Select(m => new MessageEnvelope(m, m.SessionId)).ToArray();
        byte[] serialized = envelopes.Serialize();

        IEnumerable<MessageEnvelope>? deserializedMessage = serialized.Deserialize<IEnumerable<MessageEnvelope>>();

        deserializedMessage.ShouldNotBeNull().Count().ShouldBe(envelopes.Length);
    }
}
