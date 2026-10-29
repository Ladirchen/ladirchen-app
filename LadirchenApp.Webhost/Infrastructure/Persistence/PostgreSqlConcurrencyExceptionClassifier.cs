using System;

using Npgsql;

namespace LadirchenApp.Webhost.Infrastructure.Persistence;

internal static class PostgreSqlConcurrencyExceptionClassifier
{
  public static bool IsSerializableConflict(Exception exception)
  {
    ArgumentNullException.ThrowIfNull(exception);
    var postgresException = FindPostgresException(exception);

    return postgresException?.SqlState is PostgresErrorCodes.SerializationFailure
      or PostgresErrorCodes.DeadlockDetected;
  }

  private static PostgresException? FindPostgresException(Exception exception)
  {
    Exception? current = exception;

    while (current is not null)
    {
      if (current is PostgresException postgresException)
      {
        return postgresException;
      }

      current = current.InnerException;
    }

    return null;
  }
}
