using FluentValidation;

namespace FileDB.Validators
{
    public class ConfigValidator : AbstractValidator<Config.Config>
    {
        public ConfigValidator()
        {
            RuleFor(c => c.Name).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Name is empty")
                .Length(3, 100).WithMessage("Name length is invalid");
        }
    }
}
