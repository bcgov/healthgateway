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
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Interface that defines TraceServices.
    /// </summary>
    public interface ITraceService
    {
        /// <summary>
        /// Traces the current method. If tracing is enabled will output start and end of the method.
        /// Should be called within a 'using' statement to make sure the IDisposable.Dispose method is called.
        /// Alternatively Disposed can be called manually.
        /// </summary>
        /// <param name="className">Name of the class that is being traced.</param>
        /// <param name="methodName">Name of the method that is being traced.</param>
        /// <returns>A newly created Tracer.</returns>
        ITracer TraceMethod(string className, [CallerMemberName] string methodName = "");

        /// <summary>
        /// Traces a section within a method. If tracing is enabled will output start and end of the section.
        /// Should be called within a 'using' statement to make sure the IDisposable.Dispose method is called.
        /// Alternatively Disposed can be called manually.
        /// </summary>
        /// <param name="className">Name of the class that is being traced.</param>
        /// <param name="sectionName">Name of the section that is being traced.</param>
        /// <param name="methodName">Name of the method that is being traced.</param>
        /// <returns>A newly created Tracer.</returns>
        ITracer TraceSection(string className, string sectionName = "Section", [CallerMemberName] string methodName = "");
    }
}