using System.ComponentModel.DataAnnotations;

namespace Application.AccessRules
{
    // OUTPUT
    public sealed record AccessRuleDto
    {
        public Guid Id { get; init; }
        public Guid UserGroupId { get; init; }
        public string RuleName { get; init; } = default!;
        public bool Permission { get; init; }
    }

    // INPUT
    public sealed record CreateAccessRuleDto(
        [param: Required] Guid UserGroupId,
        [param: Required, MaxLength(100)] string RuleName,
        bool Permission);
}
