using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence;
using LadirchenApp.Webhost.Models;

using Microsoft.EntityFrameworkCore;

namespace LadirchenApp.Webhost.Infrastructure;

public sealed class CurrentAuthenticationSessionReader
{
  private readonly LadirchenDbContext _database;

  public CurrentAuthenticationSessionReader(LadirchenDbContext database)
  {
    _database = database;
  }

  public Task<AuthenticationSessionResponse?> ReadAsync(
    FamilyId familyId,
    FamilyMemberId memberId,
    CancellationToken cancellationToken
  )
  {
    return _database.AuthenticationAccounts
      .AsNoTracking()
      .Where(item => item.FamilyId == familyId.Value && item.MemberId == memberId.Value)
      .Select(item => new AuthenticationSessionResponse(item.FamilyId, item.FamilyName, item.MemberId))
      .SingleOrDefaultAsync(cancellationToken);
  }
}
