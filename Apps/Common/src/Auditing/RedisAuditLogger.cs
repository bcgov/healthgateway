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
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;
    using StackExchange.Redis;

    /// <summary>
    /// A Redis Audit Logger.
    /// </summary>
    public class RedisAuditLogger : AAuditLogger
    {
        /// <summary>
        /// The name of the Redis Audit Queue.
        /// </summary>
        public const string AuditQueue = "Queue:Audit:Active";

        private readonly ILogger<DbAuditLogger> logger;
        private readonly IConnectionMultiplexer connectionMultiplexer;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisAuditLogger"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="connectionMultiplexer">The injected connection multiplexer.</param>
        public RedisAuditLogger(ILogger<DbAuditLogger> logger, IConnectionMultiplexer connectionMultiplexer)
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
            this.connectionMultiplexer.GetDatabase().ListRightPush(AuditQueue, auditJson, flags: CommandFlags.FireAndForget);
            activity?.Stop();
        }
    }
}
