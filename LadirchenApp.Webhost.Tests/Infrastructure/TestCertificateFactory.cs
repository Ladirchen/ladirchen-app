using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace LadirchenApp.Webhost.Tests.Infrastructure;

internal static class TestCertificateFactory
{
  public static string CreateEncodedPkcs12(string password)
  {
    using var key = RSA.Create(2048);
    var request = new CertificateRequest(
      "CN=Ladirchen Data Protection Test",
      key,
      HashAlgorithmName.SHA256,
      RSASignaturePadding.Pkcs1
    );
    using var certificate = request.CreateSelfSigned(
      DateTimeOffset.UtcNow.AddDays(-1),
      DateTimeOffset.UtcNow.AddDays(1)
    );

    return Convert.ToBase64String(certificate.Export(X509ContentType.Pkcs12, password));
  }
}
