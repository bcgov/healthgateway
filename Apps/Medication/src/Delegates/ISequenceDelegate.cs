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
namespace HealthGateway.Medication.Delegates
{
    /// <summary>
    /// Delegate that retrieves drugs based on the drug identifier.
    /// </summary>
    public interface ISequenceDelegate
    {
        /// <summary>
        /// Gets the next sequence number for the given sequence name.
        /// </summary>
        /// <param name="sequenceName">The sequence name</param>
        /// <returns>The next sequence value</returns>
        long NextValueForSequence(string sequenceName);
    }
}