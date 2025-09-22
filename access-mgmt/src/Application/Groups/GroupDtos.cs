using System.ComponentModel.DataAnnotations;

namespace Application.Groups
{
    // OUTPUT
    public sealed record GroupDto
    {
        public Guid Id { get; init; }
        public string GroupName { get; init; } = default!;
    }

    // INPUT
    public sealed record CreateGroupDto([property: Required, MaxLength(100)] string GroupName);
    public sealed record UpdateGroupDto([property: Required, MaxLength(100)] string GroupName);
}
