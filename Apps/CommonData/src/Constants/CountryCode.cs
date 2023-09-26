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
    /// Country codes.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Country codes should remain in all caps")]
    [SuppressMessage("ReSharper", "StringLiteralTypo", Justification = "Country names don't require spellchecking")]
    public enum CountryCode
    {
        Unknown,

        [EnumMember(Value = "Canada")]
        CA,

        [EnumMember(Value = "United States of America")]
        US,

        [EnumMember(Value = "Afghanistan")]
        AF,

        [EnumMember(Value = "Aland Islands")]
        AX,

        [EnumMember(Value = "Albania")]
        AL,

        [EnumMember(Value = "Algeria")]
        DZ,

        [EnumMember(Value = "American Samoa")]
        AS,

        [EnumMember(Value = "Andorra")]
        AD,

        [EnumMember(Value = "Angola")]
        AO,

        [EnumMember(Value = "Anguilla")]
        AI,

        [EnumMember(Value = "Antarctica")]
        AQ,

        [EnumMember(Value = "Antigua and Barbuda")]
        AG,

        [EnumMember(Value = "Argentina")]
        AR,

        [EnumMember(Value = "Armenia")]
        AM,

        [EnumMember(Value = "Aruba")]
        AW,

        [EnumMember(Value = "Australia")]
        AU,

        [EnumMember(Value = "Austria")]
        AT,

        [EnumMember(Value = "Azerbaijan")]
        AZ,

        [EnumMember(Value = "Bahamas")]
        BS,

        [EnumMember(Value = "Bahrain")]
        BH,

        [EnumMember(Value = "Bangladesh")]
        BD,

        [EnumMember(Value = "Barbados")]
        BB,

        [EnumMember(Value = "Belarus")]
        BY,

        [EnumMember(Value = "Belgium")]
        BE,

        [EnumMember(Value = "Belize")]
        BZ,

        [EnumMember(Value = "Benin")]
        BJ,

        [EnumMember(Value = "Bermuda")]
        BM,

        [EnumMember(Value = "Bhutan")]
        BT,

        [EnumMember(Value = "Bolivia")]
        BO,

        [EnumMember(Value = "Bosnia-Herzegovina")]
        BA,

        [EnumMember(Value = "Botswana")]
        BW,

        [EnumMember(Value = "Bouvet Island")]
        BV,

        [EnumMember(Value = "Brazil")]
        BR,

        [EnumMember(Value = "British Virgin Islands")]
        VG,

        [EnumMember(Value = "Brunei Darussalam")]
        BN,

        [EnumMember(Value = "Bulgaria")]
        BG,

        [EnumMember(Value = "Burkina Faso")]
        BF,

        [EnumMember(Value = "Burundi")]
        BI,

        [EnumMember(Value = "Cambodia")]
        KH,

        [EnumMember(Value = "Cameroon")]
        CM,

        [EnumMember(Value = "Cape Verde")]
        CV,

        [EnumMember(Value = "Cayman Islands")]
        KY,

        [EnumMember(Value = "Central African Republic")]
        CF,

        [EnumMember(Value = "Chad")]
        TD,

        [EnumMember(Value = "Chile")]
        CL,

        [EnumMember(Value = "China")]
        CN,

        [EnumMember(Value = "Christmas Island")]
        CX,

        [EnumMember(Value = "Cocos (Keeling) Island")]
        CC,

        [EnumMember(Value = "Colombia")]
        CO,

        [EnumMember(Value = "Comoros")]
        KM,

        [EnumMember(Value = "Congo (Republic)")]
        CG,

        [EnumMember(Value = "Congo, Democratic Republic Of")]
        CD,

        [EnumMember(Value = "Cook Islands")]
        CK,

        [EnumMember(Value = "Costa Rica")]
        CR,

        [EnumMember(Value = "Côte d'Ivoire")]
        CI,

        [EnumMember(Value = "Croatia")]
        HR,

        [EnumMember(Value = "Cuba")]
        CU,

        [EnumMember(Value = "Curacao")]
        CW,

        [EnumMember(Value = "Cyprus")]
        CY,

        [EnumMember(Value = "Czech Republic")]
        CZ,

        [EnumMember(Value = "Denmark ")]
        DK,

        [EnumMember(Value = "Djibouti")]
        DJ,

        [EnumMember(Value = "Dominica")]
        DM,

        [EnumMember(Value = "Dominican Republic")]
        DO,

        [EnumMember(Value = "Dutch Caribbean (formerly Netherlands Antilles)")]
        BQ,

        [EnumMember(Value = "Ecuador")]
        EC,

        [EnumMember(Value = "Egypt")]
        EG,

        [EnumMember(Value = "El Salvador")]
        SV,

        [EnumMember(Value = "Equatorial Guinea")]
        GQ,

        [EnumMember(Value = "Eritrea")]
        ER,

        [EnumMember(Value = "Estonia")]
        EE,

        [EnumMember(Value = "Eswatini (formerly known as Swaziland)")]
        SZ,

        [EnumMember(Value = "Ethiopia")]
        ET,

        [EnumMember(Value = "Falkland Islands")]
        FK,

        [EnumMember(Value = "Faroe Islands")]
        FO,

        [EnumMember(Value = "Fiji")]
        FJ,

        [EnumMember(Value = "Finland")]
        FI,

        [EnumMember(Value = "France")]
        FR,

        [EnumMember(Value = "French Guinea")]
        GF,

        [EnumMember(Value = "French Polynesia")]
        PF,

        [EnumMember(Value = "French Southern Territories")]
        TF,

        [EnumMember(Value = "Gabon")]
        GA,

        [EnumMember(Value = "Gambia")]
        GM,

        [EnumMember(Value = "Georgia")]
        GE,

        [EnumMember(Value = "Germany")]
        DE,

        [EnumMember(Value = "Ghana")]
        GH,

        [EnumMember(Value = "Gibraltar")]
        GI,

        [EnumMember(Value = "Greece")]
        GR,

        [EnumMember(Value = "Greenland")]
        GL,

        [EnumMember(Value = "Grenada")]
        GD,

        [EnumMember(Value = "Guadeloupe")]
        GP,

        [EnumMember(Value = "Guam")]
        GU,

        [EnumMember(Value = "Guatemala")]
        GT,

        [EnumMember(Value = "Guernsey")]
        GG,

        [EnumMember(Value = "Guinea")]
        GN,

        [EnumMember(Value = "Guinea-Bissau")]
        GW,

        [EnumMember(Value = "Guyana")]
        GY,

        [EnumMember(Value = "Haiti")]
        HT,

        [EnumMember(Value = "Heard & McDonald Islands")]
        HM,

        [EnumMember(Value = "Holy See (Vatican)")]
        VA,

        [EnumMember(Value = "Honduras")]
        HN,

        [EnumMember(Value = "Hong Kong")]
        HK,

        [EnumMember(Value = "Hungary")]
        HU,

        [EnumMember(Value = "Iceland")]
        IS,

        [EnumMember(Value = "India")]
        IN,

        [EnumMember(Value = "Indonesia")]
        ID,

        [EnumMember(Value = "Iran")]
        IR,

        [EnumMember(Value = "Iraq")]
        IQ,

        [EnumMember(Value = "Ireland")]
        IE,

        [EnumMember(Value = "Isle of Man")]
        IM,

        [EnumMember(Value = "Israel")]
        IL,

        [EnumMember(Value = "Italy")]
        IT,

        [EnumMember(Value = "Jamaica")]
        JM,

        [EnumMember(Value = "Japan")]
        JP,

        [EnumMember(Value = "Jersey")]
        JE,

        [EnumMember(Value = "Jordan")]
        JO,

        [EnumMember(Value = "Kazakhstan")]
        KZ,

        [EnumMember(Value = "Kenya")]
        KE,

        [EnumMember(Value = "Kiribati")]
        KI,

        [EnumMember(Value = "Korea (Democratic People's Republic)")]
        KP,

        [EnumMember(Value = "Korea (South)")]
        KR,

        [EnumMember(Value = "Kosovo")]
        XZ,

        [EnumMember(Value = "Kuwait")]
        KW,

        [EnumMember(Value = "Kyrgyzstan")]
        KG,

        [EnumMember(Value = "Lao (People's Democratic Republic)")]
        LA,

        [EnumMember(Value = "Latvia")]
        LV,

        [EnumMember(Value = "Lebanon")]
        LB,

        [EnumMember(Value = "Lesotho")]
        LS,

        [EnumMember(Value = "Liberia")]
        LR,

        [EnumMember(Value = "Libya")]
        LY,

        [EnumMember(Value = "Liechtenstein")]
        LI,

        [EnumMember(Value = "Lithuania")]
        LT,

        [EnumMember(Value = "Luxembourg")]
        LU,

        [EnumMember(Value = "Macau")]
        MO,

        [EnumMember(Value = "Macedonia")]
        MK,

        [EnumMember(Value = "Madagascar")]
        MG,

        [EnumMember(Value = "Malawi")]
        MW,

        [EnumMember(Value = "Malaysia")]
        MY,

        [EnumMember(Value = "Maldives")]
        MV,

        [EnumMember(Value = "Mali")]
        ML,

        [EnumMember(Value = "Malta")]
        MT,

        [EnumMember(Value = "Marshall Islands")]
        MH,

        [EnumMember(Value = "Martinique")]
        MQ,

        [EnumMember(Value = "Mauritania")]
        MR,

        [EnumMember(Value = "Mauritius")]
        MU,

        [EnumMember(Value = "Mayotte")]
        YT,

        [EnumMember(Value = "Mexico")]
        MX,

        [EnumMember(Value = "Micronesia, Federal States Of")]
        FM,

        [EnumMember(Value = "Moldova")]
        MD,

        [EnumMember(Value = "Monaco")]
        MC,

        [EnumMember(Value = "Mongolia")]
        MN,

        [EnumMember(Value = "Montenegro")]
        ME,

        [EnumMember(Value = "Montserrat")]
        MS,

        [EnumMember(Value = "Morocco")]
        MA,

        [EnumMember(Value = "Mozambique")]
        MZ,

        [EnumMember(Value = "Myanmar")]
        MM,

        [EnumMember(Value = "Namibia")]
        NA,

        [EnumMember(Value = "Nauru")]
        NR,

        [EnumMember(Value = "Nepal")]
        NP,

        [EnumMember(Value = "Netherlands")]
        NL,

        [EnumMember(Value = "New Caledonia")]
        NC,

        [EnumMember(Value = "New Zealand")]
        NZ,

        [EnumMember(Value = "Nicaragua")]
        NI,

        [EnumMember(Value = "Niger")]
        NE,

        [EnumMember(Value = "Nigeria")]
        NG,

        [EnumMember(Value = "Niue")]
        NU,

        [EnumMember(Value = "Norfolk Islands")]
        NF,

        [EnumMember(Value = "Northern Mariana Islands")]
        MP,

        [EnumMember(Value = "Norway")]
        NO,

        [EnumMember(Value = "Oman")]
        OM,

        [EnumMember(Value = "Pakistan")]
        PK,

        [EnumMember(Value = "Palau")]
        PW,

        [EnumMember(Value = "Panama")]
        PA,

        [EnumMember(Value = "Papua New Guinea")]
        PG,

        [EnumMember(Value = "Paraguay")]
        PY,

        [EnumMember(Value = "Peru")]
        PE,

        [EnumMember(Value = "Philippines")]
        PH,

        [EnumMember(Value = "Pitcairn")]
        PN,

        [EnumMember(Value = "Poland")]
        PL,

        [EnumMember(Value = "Portugal")]
        PT,

        [EnumMember(Value = "Puerto Rico")]
        PR,

        [EnumMember(Value = "Qatar")]
        QA,

        [EnumMember(Value = "Réunion")]
        RE,

        [EnumMember(Value = "Romania")]
        RO,

        [EnumMember(Value = "Russian Federation")]
        RU,

        [EnumMember(Value = "Rwanda")]
        RW,

        [EnumMember(Value = "Saint Barthélémy")]
        BL,

        [EnumMember(Value = "Saint Helena")]
        SH,

        [EnumMember(Value = "Saint Kitts and Nevis")]
        KN,

        [EnumMember(Value = "Saint Lucia")]
        LC,

        [EnumMember(Value = "Saint Martin (French Part)")]
        MF,

        [EnumMember(Value = "Saint Pierre and Miquelon")]
        PM,

        [EnumMember(Value = "Samoa")]
        WS,

        [EnumMember(Value = "San Marino")]
        SM,

        [EnumMember(Value = "Sao Tome and Principe")]
        ST,

        [EnumMember(Value = "Saudi Arabia")]
        SA,

        [EnumMember(Value = "Senegal")]
        SN,

        [EnumMember(Value = "Serbia")]
        RS,

        [EnumMember(Value = "Seychelles")]
        SC,

        [EnumMember(Value = "Sierra Leone")]
        SL,

        [EnumMember(Value = "Singapore")]
        SG,

        [EnumMember(Value = "Sint Maarten (Dutch Part)")]
        SX,

        [EnumMember(Value = "Slovakia")]
        SK,

        [EnumMember(Value = "Slovenia")]
        SI,

        [EnumMember(Value = "Solomon Islands")]
        SB,

        [EnumMember(Value = "Somalia")]
        SO,

        [EnumMember(Value = "South Africa")]
        ZA,

        [EnumMember(Value = "South Sudan")]
        SS,

        [EnumMember(Value = "Spain")]
        ES,

        [EnumMember(Value = "Sri Lanka")]
        LK,

        [EnumMember(Value = "St Vincent and the Grenadines")]
        VC,

        [EnumMember(Value = "Sudan")]
        SD,

        [EnumMember(Value = "Suriname")]
        SR,

        [EnumMember(Value = "Svalbard & Jan Mayen")]
        SJ,

        [EnumMember(Value = "Sweden")]
        SE,

        [EnumMember(Value = "Switzerland")]
        CH,

        [EnumMember(Value = "Syria")]
        SY,

        [EnumMember(Value = "Taiwan")]
        TW,

        [EnumMember(Value = "Tajikistan")]
        TJ,

        [EnumMember(Value = "Tanzania")]
        TZ,

        [EnumMember(Value = "Thailand")]
        TH,

        [EnumMember(Value = "Timor-Leste")]
        TL,

        [EnumMember(Value = "Togo")]
        TG,

        [EnumMember(Value = "Tokelau")]
        TK,

        [EnumMember(Value = "Tonga")]
        TO,

        [EnumMember(Value = "Trinidad and Tobago")]
        TT,

        [EnumMember(Value = "Tristan da Cunha")]
        TA,

        [EnumMember(Value = "Tunisia")]
        TN,

        [EnumMember(Value = "Turkey")]
        TR,

        [EnumMember(Value = "Turkmenistan")]
        TM,

        [EnumMember(Value = "Turks and Caicos Islands")]
        TC,

        [EnumMember(Value = "Tuvalu")]
        TV,

        [EnumMember(Value = "U.S. Minor Outlying Islands")]
        UM,

        [EnumMember(Value = "U.S. Virgin Islands")]
        VI,

        [EnumMember(Value = "Uganda")]
        UG,

        [EnumMember(Value = "Ukraine")]
        UA,

        [EnumMember(Value = "United Arab Emirates")]
        AE,

        [EnumMember(Value = "United Kingdom (Great Britain)")]
        GB,

        [EnumMember(Value = "Uruguay")]
        UY,

        [EnumMember(Value = "Uzbekistan")]
        UZ,

        [EnumMember(Value = "Vanuatu")]
        VU,

        [EnumMember(Value = "Venezuela")]
        VE,

        [EnumMember(Value = "Vietnam")]
        VN,

        [EnumMember(Value = "Wallis and Futuna")]
        WF,

        [EnumMember(Value = "West Bank and Gaza Strip")]
        PS,

        [EnumMember(Value = "Western Sahara")]
        EH,

        [EnumMember(Value = "Yemen")]
        YE,

        [EnumMember(Value = "Zambia")]
        ZM,

        [EnumMember(Value = "Zimbabwe")]
        ZW,
    }
}
