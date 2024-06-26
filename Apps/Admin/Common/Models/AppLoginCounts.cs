﻿// -------------------------------------------------------------------------
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
namespace HealthGateway.Admin.Common.Models
{
    /// <summary>
    /// Model containing login counts for Health Gateway applications.
    /// </summary>
    /// <param name="Web">Number of Health Gateway web app logins.</param>
    /// <param name="Mobile">Number of Health Gateway mobile app logins.</param>
    /// <param name="Android">Number of Health Gateway android app logins.</param>
    /// <param name="Ios">Number of Health Gateway ios app logins.</param>
    /// <param name="Salesforce">Number of Health Gateway Salesforce app logins.</param>
    public record AppLoginCounts(int Web, int Mobile, int Android, int Ios, int Salesforce);
}
