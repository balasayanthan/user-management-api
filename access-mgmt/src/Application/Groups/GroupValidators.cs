using FluentValidation;

namespace Application.Groups
{
    public sealed class CreateGroupValidator : AbstractValidator<CreateGroupDto>
    {
        public CreateGroupValidator() => RuleFor(x => x.GroupName).NotEmpty().MaximumLength(100);
    }
    public sealed class UpdateGroupValidator : AbstractValidator<UpdateGroupDto>
    {
        public UpdateGroupValidator() => RuleFor(x => x.GroupName).NotEmpty().MaximumLength(100);
    }

}
