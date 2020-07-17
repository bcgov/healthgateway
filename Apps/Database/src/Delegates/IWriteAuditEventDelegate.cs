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
namespace HealthGateway.Database.Delegates
{
    using HealthGateway.Database.Models;

    /// <summary>
    /// Delegate that writes a new audit event to the database.
    /// </summary>
    public interface IWriteAuditEventDelegate
    {
        /// <summary>
        /// Writes a audit event to the database.
        /// </summary>
        /// <param name="auditEvent">The audit event to write to the backend.</param>
        void WriteAuditEvent(AuditEvent auditEvent);
    }
}