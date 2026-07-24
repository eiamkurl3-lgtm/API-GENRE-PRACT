using FluentValidation;
using MinimalApiMovies.DTOs;

namespace MinimalApiMovies.Validations
{
    public class UserCredentialsDTOValidator : AbstractValidator<UserCredentialsDTO>
    {
        public UserCredentialsDTOValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
                .MaximumLength(150).WithMessage(ValidationUtilities.MessageMaxLength)
                .EmailAddress().WithMessage(ValidationUtilities.InvalidEmailMessage);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
                .MinimumLength(6).WithMessage(ValidationUtilities.MinLengthMessage);
        }
    }
}
