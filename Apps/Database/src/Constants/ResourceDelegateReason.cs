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
namespace HealthGateway.Database.Constants
{
    using System.Runtime.Serialization;

    /// <summary>
    /// ResourceDelegate Reasons for delegation.
    /// </summary>
    public enum ResourceDelegateReason
    {
        /// <summary>
        /// Represents a delegation for Covid Laboratory.
        /// </summary>
        [EnumMember(Value = "COVIDLab")]
        CovidLab,

        /// <summary>
        /// Represents a delegation for attested access to youth data.
        /// </summary>
        [EnumMember(Value = "Guardian")]
        Guardian,

        /// <summary>
        /// Represents a delegation via invitation by the data owner.
        /// </summary>
        [EnumMember(Value = "Invited")]
        Invited,
    }
}
