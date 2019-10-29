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
namespace HealthGateway.Common.Authorization
{
    using Microsoft.AspNetCore.Authorization.Infrastructure;

    /// <summary>
    /// The ResourceOperation for Authorization
    /// </summary>
    public static class PatientDataOperations
    {

        /// <summary>
        /// The Read resource operation 
        /// </summary>        
        public static readonly OperationAuthorizationRequirement Read =
        new OperationAuthorizationRequirement
        {
            Name = OperationConstants.ReadOperationName,
        };
    }

    /// <summary>
    /// The OperationConstants for use by <ref name="PatientDataOperations"/>
    /// </summary>
    public class OperationConstants
    {
        /// <summary>
        /// The Read operation for use by <ref name="PatientDataOperations"/>
        /// </summary>        
        public static readonly string ReadOperationName = "Read";
    }
}