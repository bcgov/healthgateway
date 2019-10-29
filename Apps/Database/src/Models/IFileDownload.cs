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
namespace HealthGateway.Database.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using HealthGateway.Database.Constant;

    /// <summary>
    /// An entity represenging a file that was processed to load tables.
    /// </summary>
    public interface IFileDownload
    {
        /// <summary>
        /// Gets or sets the unique id.
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the downloaded zip file.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the SHA256 hash of the downloaded file.
        /// </summary>
        string Hash { get; set; }

        /// <summary>
        /// Gets or sets the id representing the program processing the file.
        /// </summary>
        ProgramType ProgramTypeCodeId { get; set; }

        /// <summary>
        /// Gets or sets the Program Type Code
        /// Used to define the foreign key in code first.
        /// </summary>
        ProgramTypeCode ProgramTypeCode { get; set; }

        /// <summary>
        /// Gets or sets the local file path to store the downloaded file.
        /// </summary>
        string LocalFilePath { get; set; }
    }
}
