using System.ComponentModel.DataAnnotations;

namespace Application.Users
{
    // OUTPUT 
    public sealed record UserDto
    {
        public Guid Id { get; init; }
        public string FirstName { get; init; } = default!;
        public string LastName { get; init; } = default!;
        public string Email { get; init; } = default!;
        public Guid UserGroupId { get; init; }
        public string GroupName { get; init; } = default!;
        public int? AttachedCustomerId { get; init; }
    }

    // INPUT 
    public sealed record CreateUserDto(
        [param: Required, MaxLength(100)] string FirstName,
        [param: Required, MaxLength(100)] string LastName,
        [param: Required, EmailAddress, MaxLength(256)] string Email,
        [param: Required] Guid UserGroupId,
        int? AttachedCustomerId = null);

    public sealed record UpdateUserDto(
        [param: Required, MaxLength(100)] string FirstName,
        [param: Required, MaxLength(100)] string LastName,
        [param: Required, EmailAddress, MaxLength(256)] string Email,
        [param: Required] Guid UserGroupId,
        int? AttachedCustomerId = null);
}
