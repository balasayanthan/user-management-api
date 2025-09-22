using System.ComponentModel.DataAnnotations;

namespace Application.Users
{
    public sealed record UserDto(Guid Id, string FirstName, string LastName, string Email, Guid UserGroupId, string GroupName, int? AttachedCustomerId);
    public sealed record CreateUserDto(
        [property: Required, MaxLength(100)] string FirstName,
        [property: Required, MaxLength(100)] string LastName,
        [property: Required, EmailAddress, MaxLength(256)] string Email,
        [property: Required] Guid UserGroupId,
        int? AttachedCustomerId = null);
    public sealed record UpdateUserDto(
        [property: Required, MaxLength(100)] string FirstName,
        [property: Required, MaxLength(100)] string LastName,
        [property: Required, EmailAddress, MaxLength(256)] string Email,
        [property: Required] Guid UserGroupId,
        int? AttachedCustomerId = null);
}
