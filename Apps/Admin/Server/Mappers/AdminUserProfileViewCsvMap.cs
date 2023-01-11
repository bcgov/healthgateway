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
namespace HealthGateway.Admin.Server.Mappers
{
    using System.Globalization;
    using CsvHelper.Configuration;
    using HealthGateway.Admin.Server.Converters;
    using HealthGateway.Admin.Server.Models;

    /// <summary>
    /// Maps the AdminUserProfileView model to a CSV.
    /// </summary>
    public sealed class AdminUserProfileViewCsvMap : ClassMap<AdminUserProfileView>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdminUserProfileViewCsvMap"/> class.
        /// </summary>
        public AdminUserProfileViewCsvMap()
        {
            this.AutoMap(CultureInfo.InvariantCulture);
            this.Map(m => m.AdminUserProfileId).Ignore();
            this.Map(m => m.UserId).Ignore();
            this.Map(m => m.LastLoginDateTime).TypeConverter(new UtcPstDateOutputConverter());
        }
    }
}
