using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Commands;

public sealed record FamilyUserNotification(
  FamilyMemberId RecipientId,
  string TitleDe,
  string BodyDe,
  string TitleEn,
  string BodyEn,
  string Url,
  string Tag
);