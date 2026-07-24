using FluentValidation;
using MinimalApiMovies.DTOs;

namespace MinimalApiMovies.Validations
{
    public class EditClaimDTOValidator : AbstractValidator<EditClaimDTO>
    {
        public EditClaimDTOValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
                .MaximumLength(150).WithMessage(ValidationUtilities.MessageMaxLength)
                .EmailAddress().WithMessage(ValidationUtilities.InvalidEmailMessage);
        }
    }
}
