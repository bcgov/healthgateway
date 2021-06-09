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
namespace HealthGateway.Database.Constants
{
    /// <summary>
    /// Represents the status of a Wallet Connection.
    /// </summary>
    public enum WalletConnectionStatus
    {
        /// <summary>
        /// Constant value to a Wallet connection has been initiated.
        /// </summary>
        Pending,

        /// <summary>
        /// Constant value to represent a Connected state.
        /// </summary>
        Connected,

        /// <summary>
        /// Constant value to represent that the wallet connection has been terminated.
        /// </summary>
        Disconnected,
    }
}
