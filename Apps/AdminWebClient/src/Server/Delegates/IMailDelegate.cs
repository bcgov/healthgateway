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
namespace HealthGateway.Common.Delegates
{
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.CDogs;

    /// <summary>
    /// Defines a delegate to mail documents.
    /// </summary>
    public interface IMailDelegate
    {
        /// <summary>
        /// Sends a document to be mailed.
        /// </summary>
        /// <param name="document">The document to be mailed.</param>
        /// <returns>True if the document was sucessfully transferred.</returns>
        PrimitiveRequestResult<bool> SendDocument(ReportModel document);
    }
}
