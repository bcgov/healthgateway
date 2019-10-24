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
namespace HealthGateway.Database.Constant
{
    #pragma warning disable CA1707 // The name of an identifier contains the underscore (_) character.
    #pragma warning disable SA1310 // A field name in C# contains an underscore.
    public static class Sequence
    {
        /// <summary>
        /// The DB name for the Pharmanet Trace ID Sequence.
        /// </summary>
        public const string PHARMANET_TRACE = @"trace_seq";
    }
    #pragma warning restore SA1310
    #pragma warning restore CA1707
}