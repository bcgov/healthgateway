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
namespace HealthGateway.Patient.Constants
{
    using System;

    /// <summary>
    /// A class with constants representing the various OID values.
    /// </summary>
    public class OIDType
    {
        /// <summary>
        /// Gets or sets the value that holds the internal representation of the OIDType.
        /// </summary>
        private readonly string value;

        private OIDType(string value)
        {
            this.value = value;
        }

        /// <summary>
        /// Gets an OID representing hdid.
        /// </summary>
        public static OIDType HDID
        {
            get { return new OIDType("2.16.840.1.113883.3.51.1.1.6"); }
        }

        /// <summary>
        /// Gets an OID representing hdid.
        /// </summary>
        public static OIDType PHN
        {
            get { return new OIDType("2.16.840.1.113883.3.51.1.1.6.1"); }
        }

        /// <summary>
        /// Determines whether two specified types have the same value.
        /// </summary>
        /// <returns>True if the value of a is equal from the value of b; otherwise, false.</returns>
        /// <param name="lhs">The first type to compare, or null.</param>
        /// <param name="rhs">The second type to compare, or null.</param>
        public static bool operator ==(OIDType lhs, OIDType rhs)
        {
            // Check for null on left side.
            if (object.ReferenceEquals(lhs, null))
            {
                if (object.ReferenceEquals(rhs, null))
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

        /// <summary>
        /// Determines whether two specified types have different values.
        /// </summary>
        /// <returns>True if the value of a is different from the value b; otherwise, false.</returns>
        /// <param name="lhs">The first type to compare, or null.</param>
        /// <param name="rhs">The second type to compare, or null.</param>
        public static bool operator !=(OIDType lhs, OIDType rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Definition of the equal operator.
        /// </summary>
        /// <returns>True if the value of a is equal; otherwise, false.</returns>
        /// <param name="obj">The object to compare against for equality.</param>
        public override bool Equals(object? obj)
        {
            // Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                OIDType p = (OIDType)obj;
                return this.value == p.value;
            }
        }

        /// <summary>
        /// Gets the hash code for this OIDType.
        /// </summary>
        /// <returns>The computed hash code.</returns>
        public override int GetHashCode()
        {
            return this.value.GetHashCode(System.StringComparison.InvariantCulture);
        }

        /// <summary>
        /// Gets string representation of this type.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return this.value;
        }
    }
}