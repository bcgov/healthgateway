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
namespace HealthGateway.WebClient.Services
{
    using System.Collections.Generic;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Service to interact with the Note Delegate.
    /// </summary>
    public interface INoteService
    {
        /// <summary>
        /// Creates a note in the backend.
        /// </summary>
        /// <param name="note">The note to create.</param>
        /// <returns>A note wrapped in a RequestResult.</returns>
        public RequestResult<Note> CreateNote(Note note);

        /// <summary>
        /// Gets all the notes for the given hdId.
        /// </summary>
        /// <param name="hdId">The users HDID.</param>
        /// <param name="page">The page of data to fetch indexed from 0.</param>
        /// <param name="pageSize">The amount of records per page.</param>
        /// <returns>A List of notes wrapped in a RequestResult.</returns>
        public RequestResult<IEnumerable<Note>> GetNotes(string hdId, int page = 0, int pageSize = 500);
    }
}
