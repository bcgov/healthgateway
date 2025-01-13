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
namespace HealthGateway.CommonTests.Utils
{
    using System;
    using System.IO;
    using HealthGateway.Common.Utils;
    using Org.BouncyCastle.Asn1.Sec;
    using Org.BouncyCastle.Asn1.X509;
    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Crypto.Generators;
    using Org.BouncyCastle.Crypto.Operators;
    using Org.BouncyCastle.Crypto.Parameters;
    using Org.BouncyCastle.Math;
    using Org.BouncyCastle.Pkcs;
    using Org.BouncyCastle.Security;
    using Org.BouncyCastle.Utilities;
    using Org.BouncyCastle.X509;
    using Xunit;

    /// <summary>
    /// CertificateConverterTests.
    /// </summary>
    public class CertificateConverterTests
    {
        private const string ValidPassword = "testPassword";
        private static readonly byte[] ValidPfxData = GenerateMockPfx();

        [Fact]
        public void ShouldConvertPfxToPem()
        {
            // Act
            (string CertificatePem, string PrivateKeyPem) result = CertificateConverter.ConvertPfxToPem(ValidPfxData, ValidPassword);

            // Assert
            Assert.False(string.IsNullOrEmpty(result.CertificatePem), "Certificate PEM should not be empty.");
            Assert.False(string.IsNullOrEmpty(result.PrivateKeyPem), "Private Key PEM should not be empty.");
            Assert.Contains("BEGIN CERTIFICATE", result.CertificatePem, StringComparison.Ordinal);
            Assert.Contains("BEGIN RSA PRIVATE KEY", result.PrivateKeyPem, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData(null)] // Test case for null PFX data
        [InlineData(new byte[0])] // Test case for empty PFX data
        public void ConvertPfxToPemThrowsArgumentExceptionGivenInvalidPfxData(byte[]? invalidPfxData)
        {
            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => CertificateConverter.ConvertPfxToPem(invalidPfxData, ValidPassword));
            Assert.Equal("PFX data cannot be null or empty. (Parameter 'pfxData')", exception.Message);
        }

        [Theory]
        [InlineData(null)] // Test case for null password
        [InlineData("")] // Test case for empty password
        public void ConvertPfxToPemThrowsArgumentExceptionGivenInvalidPassword(string? invalidPassword)
        {
            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => CertificateConverter.ConvertPfxToPem(ValidPfxData, invalidPassword));
            Assert.Equal("Password cannot be null or empty. (Parameter 'password')", exception.Message);
        }

        [Fact]
        public void ConvertPfxToPemThrowsIoExceptionGivenInvalidPassword()
        {
            // Act & Assert
            IOException exception = Assert.Throws<IOException>(
                () => CertificateConverter.ConvertPfxToPem(ValidPfxData, "wrongPassword"));
            Assert.Equal("PKCS12 key store MAC invalid - wrong password or corrupted file.", exception.Message);
        }

        [Fact]
        public void ConvertPfxToPemThrowsInvalidOperationExceptionWhenNoPrivateKey()
        {
            // Arrange: Generate a mock PFX without a private key
            byte[] pfxWithoutPrivateKey = GenerateMockPfx(privateKeyExists: false);

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => CertificateConverter.ConvertPfxToPem(pfxWithoutPrivateKey, ValidPassword));

            Assert.Equal("No private key found in the PFX file.", exception.Message);
        }

        [Fact]
        public void ConvertPfxToPemThrowsInvalidOperationExceptionForNonRsaPrivateKey()
        {
            // Arrange: Generate a mock PFX with a non-RSA private key
            byte[] pfxWithNonRsaKey = GenerateMockPfx(useRsaKey: false);

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => CertificateConverter.ConvertPfxToPem(pfxWithNonRsaKey, ValidPassword));
            Assert.Equal("Only RSA private keys are supported.", exception.Message);
        }

        [Fact]
        public void ConvertPfxToPemThrowsIoExceptionGivenInvalidPfxData()
        {
            // Arrange
            byte[] invalidPfxData = [0x00, 0x01, 0x02];

            // Act & Assert
            Assert.Throws<IOException>(
                () =>
                    CertificateConverter.ConvertPfxToPem(invalidPfxData, ValidPassword));
        }

        private static byte[] GenerateMockPfx(bool useRsaKey = true, bool privateKeyExists = true)
        {
            AsymmetricCipherKeyPair keyPair;

            // Generate either RSA or EC key pair
            if (useRsaKey)
            {
                RsaKeyPairGenerator rsaKeyPairGenerator = new();
                rsaKeyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
                keyPair = rsaKeyPairGenerator.GenerateKeyPair();
            }
            else
            {
                ECKeyPairGenerator ecKeyPairGenerator = new();
                ecKeyPairGenerator.Init(new ECKeyGenerationParameters(SecObjectIdentifiers.SecP256r1, new SecureRandom()));
                keyPair = ecKeyPairGenerator.GenerateKeyPair();
            }

            // Create a self-signed certificate
            X509V3CertificateGenerator certificateGenerator = new();
            BigInteger serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(long.MaxValue), new SecureRandom());
            certificateGenerator.SetSerialNumber(serialNumber);
            certificateGenerator.SetIssuerDN(new X509Name("CN=Test Certificate"));
            certificateGenerator.SetSubjectDN(new X509Name("CN=Test Certificate"));
            certificateGenerator.SetNotBefore(DateTime.UtcNow.Date);
            certificateGenerator.SetNotAfter(DateTime.UtcNow.Date.AddYears(1));
            certificateGenerator.SetPublicKey(keyPair.Public);

            // Use a compatible signature factory for RSA or EC
            string signatureAlgorithm = useRsaKey ? "SHA256WITHRSA" : "SHA256WITHECDSA";
            ISignatureFactory signatureFactory = new Asn1SignatureFactory(signatureAlgorithm, keyPair.Private, new SecureRandom());

            X509Certificate certificate = certificateGenerator.Generate(signatureFactory);

            // Create a Pkcs12Store and add the key pair and certificate
            Pkcs12Store store = new Pkcs12StoreBuilder().Build();

            if (privateKeyExists)
            {
                X509CertificateEntry certificateEntry = new(certificate);
                store.SetKeyEntry("testAlias", new AsymmetricKeyEntry(keyPair.Private), [certificateEntry]);
            }
            else
            {
                store.SetCertificateEntry("testCert", new X509CertificateEntry(certificate));
            }

            // Export the store to a byte array
            MemoryStream memoryStream = new();
            store.Save(memoryStream, ValidPassword.ToCharArray(), new SecureRandom());
            return memoryStream.ToArray();
        }
    }
}
