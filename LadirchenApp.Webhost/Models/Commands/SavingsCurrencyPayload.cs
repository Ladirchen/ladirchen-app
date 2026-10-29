using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record SavingsCurrencyPayload([property: JsonRequired] string Currency);
