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
namespace HealthGateway.Common.Utils
{
    using System;
    using System.IO;
    using System.Linq;
    using Org.BouncyCastle.Crypto.Parameters;
    using Org.BouncyCastle.OpenSsl;
    using Org.BouncyCastle.Pkcs;

    /// <summary>
    /// Provides utility methods for managing and converting certificates.
    /// </summary>
    public static class CertificateConverter
    {
        /// <summary>
        /// Converts a password-protected PFX file to PEM format.
        /// </summary>
        /// <param name="pfxData">The PFX file as a byte array.</param>
        /// <param name="password">The password to decrypt the PFX file.</param>
        /// <returns>
        /// A tuple containing:
        /// <c>CertificatePem</c>: The PEM-formatted certificate.
        /// <c>PrivateKeyPem</c>: The PEM-formatted private key.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when pfxData is null or empty, or when the password is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no private key is found in the PFX file.</exception>
        /// <exception cref="IOException">Thrown when the PFX file is invalid or the password is incorrect.</exception>
        public static (string CertificatePem, string PrivateKeyPem) ConvertPfxToPem(byte[]? pfxData, string? password)
        {
            if (pfxData == null || pfxData.Length == 0)
            {
                throw new ArgumentException("PFX data cannot be null or empty.", nameof(pfxData));
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            }

            // Load the PFX data into a Pkcs12Store using a memory stream
            Pkcs12Store pkcs12Store;
            using (MemoryStream memoryStream = new(pfxData))
            {
                pkcs12Store = new Pkcs12StoreBuilder().Build();
                pkcs12Store.Load(memoryStream, password.ToCharArray());
            }

            // Find the alias containing the private key
            string alias = pkcs12Store.Aliases.FirstOrDefault(pkcs12Store.IsKeyEntry)
                           ?? throw new InvalidOperationException("No private key found in the PFX file.");

            // Extract the certificate and private key
            X509CertificateEntry certificateEntry = pkcs12Store.GetCertificate(alias);
            AsymmetricKeyEntry privateKeyEntry = pkcs12Store.GetKey(alias);

            // Export the certificate and private key to PEM
            return ExportToPem(certificateEntry, privateKeyEntry);
        }

        private static (string CertificatePem, string PrivateKeyPem) ExportToPem(X509CertificateEntry certificateEntry, AsymmetricKeyEntry privateKeyEntry)
        {
            if (privateKeyEntry.Key is not RsaPrivateCrtKeyParameters rsaKey)
            {
                throw new InvalidOperationException("Only RSA private keys are supported.");
            }

            using StringWriter certificateWriter = new(), privateKeyWriter = new();

            // Write the certificate in PEM format
            using (PemWriter certificatePemWriter = new(certificateWriter))
            {
                certificatePemWriter.WriteObject(certificateEntry.Certificate);
            }

            // Write the private key in PEM format
            using (PemWriter privateKeyPemWriter = new(privateKeyWriter))
            {
                privateKeyPemWriter.WriteObject(rsaKey);
            }

            // Return PEM-formatted certificate and private key
            return (certificateWriter.ToString(), privateKeyWriter.ToString());
        }
    }
}
