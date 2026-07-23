using FluentValidation;
using MinimalApiMovies.DTOs;

namespace MinimalApiMovies.Validations
{
    public class CreateMoviesDTOValidator : AbstractValidator<CreateMoviesDTO>
    {

        public CreateMoviesDTOValidator()
        {

            RuleFor(x => x.Title)
            .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
            .MaximumLength(300).WithMessage(ValidationUtilities.MessageMaxLength)
            .Must(ValidationUtilities.FirstLetterIsUppercase).WithMessage(ValidationUtilities.UpperCaseMessage);
            RuleFor(x => x.InTheaters)
                .NotNull().WithMessage("InTheaters field is required.");
            RuleFor(x => x.ReleaseDate)
                .NotNull().WithMessage("ReleaseDate field is required.")
                .LessThanOrEqualTo(DateTime.Today).WithMessage("ReleaseDate cannot be in the future.");

        }
        
    }
}
