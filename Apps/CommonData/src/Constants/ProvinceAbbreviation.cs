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
    /// Abbreviations for Canadian provinces and territories.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Province abbreviations should remain in all caps")]
    public enum ProvinceAbbreviation
    {
        Unknown,

        [EnumMember(Value = "Alberta")]
        AB,

        [EnumMember(Value = "British Columbia")]
        BC,

        [EnumMember(Value = "Manitoba")]
        MB,

        [EnumMember(Value = "New Brunswick")]
        NB,

        [EnumMember(Value = "Newfoundland and Labrador")]
        NL,

        [EnumMember(Value = "Northwest Territories")]
        NT,

        [EnumMember(Value = "Nova Scotia")]
        NS,

        [EnumMember(Value = "Nunavut")]
        NU,

        [EnumMember(Value = "Ontario")]
        ON,

        [EnumMember(Value = "Prince Edward Island")]
        PE,

        [EnumMember(Value = "Quebec")]
        QC,

        [EnumMember(Value = "Saskatchewan")]
        SK,

        [EnumMember(Value = "Yukon Territory")]
        YT,
    }
}
