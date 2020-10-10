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
using System;

namespace HealthGateway.Patient.Constants
{
    /// <summary>
    /// A class with constants representing the various OID values.
    /// </summary>
    public class OIDType
    {
        /// <summary>
        /// Gets or sets the value that holds the internal representation of the OIDType.
        /// </summary>
        public readonly string Value;

        private OIDType(string value)
        {
            this.Value = value;
        }

        /// <summary>
        /// OID representing hdid.
        /// </summary>
        public static OIDType HDID
        {
            get { return new OIDType("2.16.840.1.113883.3.51.1.1.6"); }
        }

        /// <summary>
        /// OID representing hdid.
        /// </summary>
        public static OIDType PHN
        {
            get { return new OIDType("2.16.840.1.113883.3.51.1.1.6.1"); }
        }

        /// <summary>
        /// Definition of the equal operator.
        /// </summary>
        public override bool Equals(Object? obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                OIDType p = (OIDType)obj;
                return Value == p.Value;
            }
        }

        public static bool operator ==(OIDType lhs, OIDType rhs)
        {
            // Check for null on left side.
            if (Object.ReferenceEquals(lhs, null))
            {
                if (Object.ReferenceEquals(rhs, null))
                {
                    // null == null = true.
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(OIDType lhs, OIDType rhs)
        {
            return !(lhs == rhs);
        }
    }
}