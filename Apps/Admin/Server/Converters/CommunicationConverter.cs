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
namespace HealthGateway.Admin.Common.Models
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Converts Communication model from DB to UI and back.
    /// </summary>
    public static class CommunicationConverter
    {
        /// <summary>
        /// Gets a UI model from a DB model.
        /// </summary>
        /// <param name="communication">The DB Communication to copy into this object.</param>
        /// <returns>The Db model converted to Ui model.</returns>
        public static Communication ToUiModel(Database.Models.Communication communication)
        {
            Communication retComm = new()
            {
                Id = communication.Id,
                Text = communication.Text,
                Subject = communication.Subject,
                EffectiveDateTime = communication.EffectiveDateTime,
                ExpiryDateTime = communication.ExpiryDateTime,
                ScheduledDateTime = communication.ScheduledDateTime,
                CommunicationTypeCode = communication.CommunicationTypeCode,
                CommunicationStatusCode = communication.CommunicationStatusCode,
                Priority = communication.Priority,
                CreatedBy = communication.CreatedBy,
                CreatedDateTime = communication.CreatedDateTime,
                UpdatedBy = communication.UpdatedBy,
                UpdatedDateTime = communication.UpdatedDateTime,
                Version = communication.Version,
            };

            return retComm;
        }

        /// <summary>
        /// Converts a list of DB Communications into a list of UI Model Communications.
        /// </summary>
        /// <param name="communications">The list of DB Communications to convert.</param>
        /// <returns>A converted list or null if inbound is null.</returns>
        public static IEnumerable<Communication>? ToUiModel(IEnumerable<Database.Models.Communication> communications)
        {
            return communications.Select(CommunicationConverter.ToUiModel).ToList();
        }

        /// <summary>
        /// Gets a DB model from a UI model.
        /// </summary>
        /// <param name="communication">The DB Communication to copy into this object.</param>
        /// <returns>The Ui model converted to Db model.</returns>
        public static Database.Models.Communication ToDbModel(Communication communication)
        {
            Database.Models.Communication retComm = new()
            {
                Id = communication.Id,
                Text = communication.Text,
                Subject = communication.Subject,
                EffectiveDateTime = communication.EffectiveDateTime,
                ExpiryDateTime = communication.ExpiryDateTime,
                ScheduledDateTime = communication.ScheduledDateTime,
                CommunicationTypeCode = communication.CommunicationTypeCode,
                CommunicationStatusCode = communication.CommunicationStatusCode,
                Priority = communication.Priority,
                CreatedBy = communication.CreatedBy,
                CreatedDateTime = communication.CreatedDateTime,
                UpdatedBy = communication.UpdatedBy,
                UpdatedDateTime = communication.UpdatedDateTime,
                Version = communication.Version,
            };

            return retComm;
        }
    }
}
