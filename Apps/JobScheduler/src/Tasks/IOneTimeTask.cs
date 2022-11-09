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
namespace HealthGateway.JobScheduler.Tasks
{
    /// <summary>
    /// A task that should be invoked only once from the OneTimeJob.
    /// Any DB access should not commit and should defer to the Job to do so.
    /// </summary>
    public interface IOneTimeTask
    {
        /// <summary>
        /// Runs the task that needs to be done for the IOneTimeTask.
        /// </summary>
        void Run();
    }
}
