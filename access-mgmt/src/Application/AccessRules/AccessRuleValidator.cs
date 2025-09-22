using FluentValidation;

namespace Application.AccessRules
{
    public sealed class CreateAccessRuleValidator : AbstractValidator<CreateAccessRuleDto>
    {
        public CreateAccessRuleValidator()
        {
            RuleFor(x => x.UserGroupId).NotEmpty();
            RuleFor(x => x.RuleName).NotEmpty().MaximumLength(100);
        }
    }
}
