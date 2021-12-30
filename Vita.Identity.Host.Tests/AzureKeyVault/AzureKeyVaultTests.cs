using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Keys;
using System;
using System.Security.Cryptography;
using Xunit;

namespace Vita.Identity.Host.Tests.AzureKeyVault
{
    public class AzureKeyVaultTests
    {
        [Fact(Skip = "Azure Key Vault Concept Tests")]
        public void RSAKeyTest()
        {
            var keyClient = new KeyClient(new Uri("{{your keyvault uri}}"), new DefaultAzureCredential());
            Response<KeyVaultKey> response = keyClient.GetKey("SingInCredentialsKey");

            RSA rsa = response.Value.Key.ToRSA();

            Assert.NotNull(rsa);
        }

        [Fact(Skip = "Azure Key Vault Concept Tests")]
        public void CertificateTest()
        {
            var keyClient = new CertificateClient(new Uri("{{your keyvault uri}}"), new DefaultAzureCredential());
            Response<KeyVaultCertificateWithPolicy> response = keyClient.GetCertificate("SignInCredentialsCert");

            Assert.NotNull(response.Value.Properties.Version);
        }
    }
}
