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
namespace HealthGateway.Common.Instrumentation
{
    using System;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementation that measures time taking between initialization and disposal.
    /// </summary>
    public sealed class TimedTracer : ITracer
    {
        private readonly Stopwatch timer;
        private readonly string name;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimedTracer"/> class.
        /// </summary>
        /// <param name="logger">Provided Logger.</param>
        /// <param name="tracerName">Name that the tracer will use when logging.</param>
        public TimedTracer(ILogger logger, string tracerName)
        {
            this.name = tracerName;
            this.logger = logger;
            this.timer = new Stopwatch();
            this.Begin();
        }

        /// <summary>
        /// Stops the tracing.
        /// </summary>
        public void Dispose()
        {
            this.End();
            GC.SuppressFinalize(this);
        }

        private void Begin()
        {
            this.logger.LogTrace($"Trace:[{this.name}] Begin");
            this.timer.Start();
        }

        private void End()
        {
            this.logger.LogTrace($"Trace:[{this.name}] End - {this.timer.Elapsed}");
            this.timer.Stop();
        }
    }
}