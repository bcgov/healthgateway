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
namespace HealthGateway.Common.Utils;

using System;
using System.Globalization;
using HealthGateway.Common.Delegates;

/// <summary>
/// Utility for generating verification code.
/// </summary>
public static class VerificationCodeUtility
{
    /// <summary>
    /// Generates a random numeric code.
    /// </summary>
    /// <param name="randomNumberSize">The byte array size for random number generator, defaults to 4.</param>
    /// <param name="digits">The length of the generated numeric code returned, defaults to 6.</param>
    /// <returns>The verification code.</returns>
    public static string Generate(int randomNumberSize = 4, int digits = 6)
    {
        byte[] salt = HmacHashDelegate.GenerateSalt(randomNumberSize);

        return BitConverter
            .ToUInt32(salt)
            .ToString($"D{digits}", CultureInfo.InvariantCulture)[..digits];
    }
}
