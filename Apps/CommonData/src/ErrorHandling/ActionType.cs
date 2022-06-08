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
namespace HealthGateway.Common.Data.Models.ErrorHandling
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Enumerator that defines the different types of required actions.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ActionType
    {
        private ActionType(string value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets the protected user error.
        /// </summary>
        public static ActionType Protected
        {
            get { return new ActionType("PROTECTED"); }
        }

        /// <summary>
        /// Gets the missing hdid user error.
        /// </summary>
        public static ActionType NoHdId
        {
            get { return new ActionType("NOHDID"); }
        }

        /// <summary>
        /// Gets the missing name user error.
        /// </summary>
        public static ActionType InvalidName
        {
            get { return new ActionType("INVALIDNAME"); }
        }

        /// <summary>
        /// Gets the data mismatch user error.
        /// </summary>
        public static ActionType DataMismatch
        {
            get { return new ActionType("MISMATCH"); }
        }

        /// <summary>
        /// Gets the expired user error.
        /// </summary>
        public static ActionType Expired
        {
            get { return new ActionType("EXPIRED"); }
        }

        /// <summary>
        /// Gets the general data validation error.
        /// </summary>
        public static ActionType Invalid
        {
            get { return new ActionType("INVALID"); }
        }

        /// <summary>
        /// Gets the general data validation error.
        /// </summary>
        public static ActionType Refresh
        {
            get { return new ActionType("REFRESH"); }
        }

        /// <summary>
        /// Gets the general data validation error.
        /// </summary>
        public static ActionType Validation
        {
            get { return new ActionType("VALIDATION"); }
        }

        /// <summary>
        /// Gets the action that the request was already processed.
        /// </summary>
        public static ActionType Processed
        {
            get { return new ActionType("PROCESSED"); }
        }

        /// <summary>
        /// Gets or sets the value that holds the internal representation of the ActionType.
        /// </summary>
        public string Value { get; set; }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return this.Value == ((ActionType)obj!).Value;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return this.Value.GetHashCode(System.StringComparison.CurrentCulture);
        }
    }
}
