using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Infrastructure;

using Microsoft.Extensions.Configuration;

namespace LadirchenApp.Webhost.Tests.Infrastructure;

public sealed class DataProtectionCertificateTests
{
  [Test]
  public async Task ProductionRequiresCertificateAndPassword()
  {
    var configuration = CreateConfiguration(("VERCEL_ENV", "production"));

    await Assert.That(() =>
      DataProtectionCertificate.Load(configuration)
    ).Throws<InvalidOperationException>();
  }

  [Test]
  public async Task LocalDevelopmentCanUseUnencryptedKeyRing()
  {
    var certificate = DataProtectionCertificate.Load(CreateConfiguration());

    await Assert.That(certificate).IsNull();
  }

  [Test]
  public async Task Base64Pkcs12CertificateLoadsWithPrivateKey()
  {
    const string password = "test-certificate-password";
    var encodedCertificate = TestCertificateFactory.CreateEncodedPkcs12(password);
    var configuration = CreateConfiguration(
      (DataProtectionCertificate.CertificateBase64ConfigurationKey, encodedCertificate),
      (DataProtectionCertificate.CertificatePasswordConfigurationKey, password)
    );

    using var certificate = DataProtectionCertificate.Load(configuration)
      ?? throw new InvalidOperationException("The configured certificate was not loaded.");

    await Assert.That(certificate.HasPrivateKey).IsTrue();
  }

  [Test]
  public async Task InvalidCertificateEncodingIsReportedAsConfigurationFailure()
  {
    var configuration = CreateConfiguration(
      (DataProtectionCertificate.CertificateBase64ConfigurationKey, "not-base64"),
      (DataProtectionCertificate.CertificatePasswordConfigurationKey, "secret")
    );

    await Assert.That(() =>
      DataProtectionCertificate.Load(configuration)
    ).Throws<InvalidOperationException>();
  }

  private static IConfiguration CreateConfiguration(params (string Key, string Value)[] values)
  {
    var settings = new Dictionary<string, string?>();

    foreach (var (key, value) in values)
    {
      settings[key] = value;
    }

    return new ConfigurationBuilder()
      .AddInMemoryCollection(settings)
      .Build();
  }
}
