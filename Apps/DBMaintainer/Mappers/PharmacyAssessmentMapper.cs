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
namespace HealthGateway.DBMaintainer.Mappers
{
    using System.Diagnostics.CodeAnalysis;
    using CsvHelper.Configuration;
    using HealthGateway.Database.Models;
    using HealthGateway.DBMaintainer.Models;

    /// <summary>
    /// Performs a mapping from the read file to the model object.
    /// </summary>
    public sealed class PharmacyAssessmentMapper : ClassMap<PharmacyAssessment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PharmacyAssessmentMapper"/> class.
        /// Performs the mapping of read Pharmacy Assessment file to the db model.
        /// </summary>
        /// <param name="fileDownload">The fileDownload to map.</param>
        [SuppressMessage("ReSharper", "UnusedParameter.Local", Justification = "Required by ClassMap")]
        public PharmacyAssessmentMapper(FileDownload fileDownload)
        {
            this.Map(m => m.Pin).Index(0);
            this.Map(m => m.PharmacyAssessmentTitle).Index(1);
            this.Map(m => m.PrescriptionProvided).Index(2);
            this.Map(m => m.RedirectedToHealthCareProvider).Index(3);
        }
    }
}
