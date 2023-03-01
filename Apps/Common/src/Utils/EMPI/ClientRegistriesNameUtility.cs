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
namespace HealthGateway.Common.Utils.EMPI
{
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Common.Models;
    using ServiceReference;

    /// <summary>
    /// Utility to extract a Name object from Patient's Service request XML response.
    /// </summary>
    public abstract class ClientRegistriesNameUtility
    {
        /// <summary>
        /// Extract the name details from the name section of the client registries patient response.
        /// </summary>
        /// <param name="nameSection">XML section of patient client.</param>
        /// <returns><see cref="Name"/> constructed name object.</returns>
        public static Name ExtractName(PN nameSection)
        {
            // Extract the subject names
            List<string> givenNameList = new();
            List<string> lastNameList = new();
            foreach (ENXP name in nameSection.Items)
            {
                if (name.GetType() == typeof(engiven) && (name.qualifier == null || !name.qualifier.Contains(cs_EntityNamePartQualifier.CL)))
                {
                    givenNameList.Add(name.Text[0]);
                }
                else if (name.GetType() == typeof(enfamily) && (name.qualifier == null || !name.qualifier.Contains(cs_EntityNamePartQualifier.CL)))
                {
                    lastNameList.Add(name.Text[0]);
                }
            }

            const string delimiter = " ";
            return new Name
            {
                GivenName = givenNameList.Aggregate((i, j) => i + delimiter + j),
                Surname = lastNameList.Aggregate((i, j) => i + delimiter + j),
            };
        }
    }
}
