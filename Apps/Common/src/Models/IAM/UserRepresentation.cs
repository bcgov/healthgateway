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
namespace HealthGateway.Common.Models.IAM
{
    using System;

    public class UserRepresentation
    {
        public DateTime? createdTimestamp { get; set; }

        public string? email { get; set; }

        public string? firstName { get; set; }

        public string? lastName { get; set; }

        public string[] realmRoles { get; set; }

        public string? username { get; set; }

        public string? id { get; set; }
    }

}