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
    public sealed record CreateGroupDto([param: Required, MaxLength(100)] string GroupName);
    public sealed record UpdateGroupDto([param: Required, MaxLength(100)] string GroupName);
}
