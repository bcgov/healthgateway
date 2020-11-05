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
namespace HealthGateway.WebClient.Models
{
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Represents a Dependent Model.
    /// </summary>
    public class DependentModel
    {
        /// <summary>
        /// Gets or sets the hdid of the dependent.
        /// </summary>
        public DependentInformation DependentInformation { get; set; } = new DependentInformation();

        /// <summary>
        /// Gets or sets the owner of the hdid.
        /// </summary>
        public string OwnerId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the hdid which has delegated access to the owner Id.
        public string DelegateId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// Constructs a new DependentModel based on a PatientModel.
        /// </summary>
        /// <param name="userDeleagate">The UserDelegate model.</param>
        /// <param name="patientModel">The Patien Model to be converted.</param>
        /// <returns>The Dependent Model.</returns>
        public static DependentModel CreateFromModels(UserDelegate userDeleagate, PatientModel patientModel)
        {
            return new DependentModel()
            {
                OwnerId = userDeleagate.OwnerId,
                DelegateId = userDeleagate.DelegateId,
                Version = userDeleagate.Version,
                DependentInformation = DependentInformation.FromPatientModel(patientModel)
            };
        }

        /// <summary>
        /// Creates a new UserDelegate model based on the dependent model.
        /// </summary>
        /// <returns>A new UserDelegate model.</returns>
        public UserDelegate ToDBModel()
        {
            return new UserDelegate()
            {
                OwnerId = this.OwnerId,
                DelegateId = this.DelegateId,
                Version = this.Version,
            };
        }
    }
}
