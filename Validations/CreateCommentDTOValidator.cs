using FluentValidation;
using MinimalApiMovies.DTOs;

namespace MinimalApiMovies.Validations
{
    public class CreateCommentDTOValidator : AbstractValidator<CreateCommentsDTO>
    {
        public CreateCommentDTOValidator()
        {
            RuleFor(x => x.Body)
                .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage);
                //.MaximumLength(500).WithMessage("");
        }
    }
}
