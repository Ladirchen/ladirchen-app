using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record SavingsExchangeRatePayload([property: JsonRequired] decimal Rate);
