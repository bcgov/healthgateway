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
namespace HealthGateway.DBMaintainer.Mappers.Converters
{
    using CsvHelper;
    using CsvHelper.Configuration;
    using CsvHelper.TypeConversion;

    /// <summary>
    /// Custom converter to convert string value to equivalent bool value.
    /// </summary>
    public class BooleanConverter : DefaultTypeConverter
    {
        /// <inheritdoc/>
        public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (TryConvertToBool(text, out bool result))
            {
                return result;
            }

            return null;
        }

        private static bool TryConvertToBool(string text, out bool result)
        {
            result = false;

            if (!string.IsNullOrWhiteSpace(text))
            {
                string boolValue = text.ToUpperInvariant();

                result = boolValue switch
                {
                    "YES" => true,
                    "NO" => false,
                    _ => false,
                };

                return true;
            }

            return false;
        }
    }
}
