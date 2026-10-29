using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Microsoft.Extensions.Configuration;

namespace LadirchenApp.Webhost.Infrastructure;

public static class DataProtectionCertificate
{
  public const string CertificateBase64ConfigurationKey = "DATA_PROTECTION_CERTIFICATE_BASE64";
  public const string CertificatePasswordConfigurationKey = "DATA_PROTECTION_CERTIFICATE_PASSWORD";

  public static X509Certificate2? Load(IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);
    var encodedCertificate = configuration[CertificateBase64ConfigurationKey];
    var password = configuration[CertificatePasswordConfigurationKey];
    var required = string.Equals(
      configuration["VERCEL_ENV"],
      "production",
      StringComparison.OrdinalIgnoreCase
    );

    if (string.IsNullOrWhiteSpace(encodedCertificate))
    {
      if (required || !string.IsNullOrEmpty(password))
      {
        throw new InvalidOperationException(
          $"{CertificateBase64ConfigurationKey} and {CertificatePasswordConfigurationKey} must both be configured."
        );
      }

      return null;
    }

    if (string.IsNullOrEmpty(password))
    {
      throw new InvalidOperationException(
        $"{CertificateBase64ConfigurationKey} and {CertificatePasswordConfigurationKey} must both be configured."
      );
    }

    try
    {
      var certificateBytes = Convert.FromBase64String(encodedCertificate);
      var certificate = X509CertificateLoader.LoadPkcs12(
        certificateBytes,
        password,
        X509KeyStorageFlags.EphemeralKeySet
      );

      if (!certificate.HasPrivateKey)
      {
        certificate.Dispose();

        throw new InvalidOperationException(
          $"{CertificateBase64ConfigurationKey} must contain a certificate with a private key."
        );
      }

      return certificate;
    }
    catch (FormatException exception)
    {
      throw new InvalidOperationException(
        $"{CertificateBase64ConfigurationKey} must be a base64-encoded PKCS#12 certificate.",
        exception
      );
    }
    catch (CryptographicException exception)
    {
      throw new InvalidOperationException(
        "The configured Data Protection certificate could not be loaded.",
        exception
      );
    }
  }
}
