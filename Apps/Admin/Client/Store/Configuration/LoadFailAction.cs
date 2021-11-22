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
namespace HealthGateway.Admin.Client.Store.Configuration
{
    /// <summary>
    /// The action representing a failed load.
    /// </summary>
    public class LoadFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadFailAction"/> class.
        /// </summary>
        /// <param name="errorMessage">The error.</param>
        public LoadFailAction(string errorMessage) => this.ErrorMessage = errorMessage;

        /// <summary>
        /// Gets the error message associated with the failed action.
        /// </summary>
        public string ErrorMessage { get; }
    }
}
