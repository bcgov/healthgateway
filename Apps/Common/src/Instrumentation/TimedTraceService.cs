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
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementation that outputs execution time traces.
    /// </summary>
    public class TimedTraceService : ITraceService
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimedTraceService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        public TimedTraceService(ILogger<TimedTraceService> logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc/>
        public ITracer TraceMethod(string className, string methodName)
        {
            string tracerName = className + "." + methodName;
            return new TimedTracer(this.logger, tracerName);
        }

        /// <inheritdoc/>
        public ITracer TraceSection(string className, string sectionName, string methodName)
        {
            string tracerName = className + "." + methodName + "." + sectionName;
            return new TimedTracer(this.logger, tracerName);
        }
    }
}