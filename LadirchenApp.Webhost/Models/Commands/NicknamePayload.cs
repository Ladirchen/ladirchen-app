using System.Text.Json.Serialization;

namespace LadirchenApp.Webhost.Models.Commands;

public sealed record NicknamePayload([property: JsonRequired] string Nickname);
