//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Common.Auditing
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;
    using StackExchange.Redis;

    /// <summary>
    /// A Redis Audit Logger.
    /// </summary>
    public class RedisAuditLogger : AuditLogger
    {
        /// <summary>
        /// The Prefix name of the Redis Audit Queue which should be used as the hash key for Redis.
        /// </summary>
        public const string AuditQueuePrefix = "{Queue:Audit}";

        /// <summary>
        /// The queue name for active audit records
        /// Active are those written to the queue but not recorded in the db.
        /// </summary>
        public const string ActiveQueueName = "Active";

        /// <summary>
        /// The queue name to use to store audit records while being written to the db.
        /// </summary>
        public const string ProcessingQueueName = "Processing";
        private readonly IConnectionMultiplexer connectionMultiplexer;

        private readonly ILogger<RedisAuditLogger> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisAuditLogger"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="connectionMultiplexer">The injected connection multiplexer.</param>
        public RedisAuditLogger(ILogger<RedisAuditLogger> logger, IConnectionMultiplexer connectionMultiplexer)
        {
            this.logger = logger;
            this.connectionMultiplexer = connectionMultiplexer;
        }

        private static ActivitySource Source { get; } = new(nameof(RedisAuditLogger));

        /// <inheritdoc/>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Team Decision")]
        public override void WriteAuditEvent(AuditEvent auditEvent)
        {
            this.logger.LogDebug(@"Writing Audit Event to Redis)");
            using Activity? activity = Source.StartActivity();
            auditEvent.CreatedDateTime = DateTime.UtcNow;
            auditEvent.UpdatedDateTime = auditEvent.CreatedDateTime;
            string auditJson = JsonSerializer.Serialize(auditEvent);
            this.connectionMultiplexer.GetDatabase().ListRightPush($"{AuditQueuePrefix}:{ActiveQueueName}", auditJson, flags: CommandFlags.FireAndForget);
            activity?.Stop();
        }
    }
}
