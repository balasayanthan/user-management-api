using System.ComponentModel.DataAnnotations;

namespace Application.AccessRules
{
    public sealed record AccessRuleDto(Guid Id, Guid UserGroupId, string RuleName, bool Permission);
    public sealed record CreateAccessRuleDto([property: Required] Guid UserGroupId, [property: Required, MaxLength(100)] string RuleName, bool Permission);
}
