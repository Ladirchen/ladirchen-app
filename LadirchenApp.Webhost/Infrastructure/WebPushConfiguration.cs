using System;

using Microsoft.Extensions.Configuration;

namespace LadirchenApp.Webhost.Infrastructure;

public sealed record WebPushConfiguration(string Subject, string PublicKey, string PrivateKey)
{
  public static WebPushConfiguration? Resolve(IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);
    var subject = configuration["WEB_PUSH_VAPID_SUBJECT"];
    var publicKey = configuration["WEB_PUSH_VAPID_PUBLIC_KEY"];
    var privateKey = configuration["WEB_PUSH_VAPID_PRIVATE_KEY"];

    return string.IsNullOrWhiteSpace(subject)
      || string.IsNullOrWhiteSpace(publicKey)
      || string.IsNullOrWhiteSpace(privateKey)
      ? null
      : new WebPushConfiguration(subject, publicKey, privateKey);
  }
}