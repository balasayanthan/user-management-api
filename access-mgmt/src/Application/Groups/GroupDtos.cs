using System.ComponentModel.DataAnnotations;

namespace Application.Groups
{
    public sealed record GroupDto(Guid Id, string GroupName);
    public sealed record CreateGroupDto([property: Required, MaxLength(100)] string GroupName);
    public sealed record UpdateGroupDto([property: Required, MaxLength(100)] string GroupName);
}
