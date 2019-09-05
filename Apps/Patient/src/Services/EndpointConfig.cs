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
namespace ServiceReference
{
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    public partial class QUPA_AR101102_PortTypeClient
    {
        static partial void ConfigureEndpoint(System.ServiceModel.Description.ServiceEndpoint serviceEndpoint, System.ServiceModel.Description.ClientCredentials clientCredentials)
        {
            clientCredentials.ServiceCertificate.SslCertificateAuthentication =
                new X509ServiceCertificateAuthentication()
                {
                    CertificateValidationMode = X509CertificateValidationMode.None,
                    RevocationMode = X509RevocationMode.NoCheck,
                };

            string clientCertificatePath = "/home/dev/Development/tmp_folder/cert/HGWAY_HI1.pfx";
            string certificatePassword = "******";
            X509Certificate2 clientCertificate = new X509Certificate2(System.IO.File.ReadAllBytes(clientCertificatePath), certificatePassword);
            clientCredentials.ClientCertificate.Certificate = clientCertificate;

            BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
            serviceEndpoint.Binding = binding;

            var a = 3;
        }
    }
}