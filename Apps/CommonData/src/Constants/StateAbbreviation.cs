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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1602 // Enumeration items should be documented

namespace HealthGateway.Common.Data.Constants
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Abbreviations for American states.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "State abbreviations should remain in all caps")]
    public enum StateAbbreviation
    {
        Unknown,

        [EnumMember(Value = "Alabama")]
        AL,

        [EnumMember(Value = "Alaska")]
        AK,

        [EnumMember(Value = "Arizona")]
        AZ,

        [EnumMember(Value = "Arkansas")]
        AR,

        [EnumMember(Value = "California")]
        CA,

        [EnumMember(Value = "Colorado")]
        CO,

        [EnumMember(Value = "Connecticut")]
        CT,

        [EnumMember(Value = "Delaware")]
        DE,

        [EnumMember(Value = "District of Columbia")]
        DC,

        [EnumMember(Value = "Florida")]
        FL,

        [EnumMember(Value = "Georgia")]
        GA,

        [EnumMember(Value = "Hawaii")]
        HI,

        [EnumMember(Value = "Idaho")]
        ID,

        [EnumMember(Value = "Illinois")]
        IL,

        [EnumMember(Value = "Indiana")]
        IN,

        [EnumMember(Value = "Iowa")]
        IA,

        [EnumMember(Value = "Kansas")]
        KS,

        [EnumMember(Value = "Kentucky")]
        KY,

        [EnumMember(Value = "Louisiana")]
        LA,

        [EnumMember(Value = "Maine")]
        ME,

        [EnumMember(Value = "Maryland")]
        MD,

        [EnumMember(Value = "Massachusetts")]
        MA,

        [EnumMember(Value = "Michigan")]
        MI,

        [EnumMember(Value = "Minnesota")]
        MN,

        [EnumMember(Value = "Mississippi")]
        MS,

        [EnumMember(Value = "Missouri")]
        MO,

        [EnumMember(Value = "Montana")]
        MT,

        [EnumMember(Value = "Nebraska")]
        NE,

        [EnumMember(Value = "Nevada")]
        NV,

        [EnumMember(Value = "New Hampshire")]
        NH,

        [EnumMember(Value = "New Jersey")]
        NJ,

        [EnumMember(Value = "New Mexico")]
        NM,

        [EnumMember(Value = "New York")]
        NY,

        [EnumMember(Value = "North Carolina")]
        NC,

        [EnumMember(Value = "North Dakota")]
        ND,

        [EnumMember(Value = "Ohio")]
        OH,

        [EnumMember(Value = "Oklahoma")]
        OK,

        [EnumMember(Value = "Oregon")]
        OR,

        [EnumMember(Value = "Pennsylvania")]
        PA,

        [EnumMember(Value = "Rhode Island")]
        RI,

        [EnumMember(Value = "South Carolina")]
        SC,

        [EnumMember(Value = "South Dakota")]
        SD,

        [EnumMember(Value = "Tennessee")]
        TN,

        [EnumMember(Value = "Texas")]
        TX,

        [EnumMember(Value = "Utah")]
        UT,

        [EnumMember(Value = "Vermont")]
        VT,

        [EnumMember(Value = "Virginia")]
        VA,

        [EnumMember(Value = "Washington")]
        WA,

        [EnumMember(Value = "West Virginia")]
        WV,

        [EnumMember(Value = "Wisconsin")]
        WI,

        [EnumMember(Value = "Wyoming")]
        WY,
    }
}
