using System;

namespace LadirchenApp.Webhost.Tests.Application.Commands;

internal sealed class FixedTimeProvider : TimeProvider
{
  private readonly DateTimeOffset _utcNow;

  public FixedTimeProvider(DateTimeOffset utcNow)
  {
    _utcNow = utcNow;
  }

  public override DateTimeOffset GetUtcNow()
  {
    return _utcNow;
  }
}