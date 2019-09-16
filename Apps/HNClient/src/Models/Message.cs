// //-------------------------------------------------------------------------
// // Copyright © 2019 Province of British Columbia
// //
// // Licensed under the Apache License, Version 2.0 (the "License");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //-------------------------------------------------------------------------
namespace HealthGateway.HNClient.Models
{
    /// <summary>
    /// A simple model object representing a response and error state from HNSecure.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Gets or sets the HL7 2.3 response message.
        /// </summary>
        public string Reply { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if an error occurred.
        /// </summary>
        public bool IsErr { get; set; }

        /// <summary>
        /// Gets or sets the error message associated to this Message.
        /// </summary>
        public string Error { get; set; }
    }
}
