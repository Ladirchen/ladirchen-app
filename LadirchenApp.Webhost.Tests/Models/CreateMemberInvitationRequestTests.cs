using System.Text.Json;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Models;

namespace LadirchenApp.Webhost.Tests.Models;

public sealed class CreateMemberInvitationRequestTests
{
  private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

  [Test]
  public async Task ChildInvitationDoesNotRequireGuardianAccess()
  {
    const string json =
      """
      {
        "email": "child@example.test",
        "name": "Child",
        "role": "child"
      }
      """;

    var request = JsonSerializer.Deserialize<CreateMemberInvitationRequest>(json, _serializerOptions);

    await Assert.That(request).IsNotNull();
    await Assert.That(request!.GuardianAccess).IsNull();
  }

  [Test]
  public async Task InvitationStillRequiresTheMemberIdentityFields()
  {
    const string json =
      """
      {
        "guardianAccess": null,
        "name": "Child",
        "role": "child"
      }
      """;

    await Assert.That(() => JsonSerializer.Deserialize<CreateMemberInvitationRequest>(json, _serializerOptions))
      .Throws<JsonException>();
  }
}
