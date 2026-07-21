using FluentValidation;
using MinimalApiMovies.DTOs;
using MinimalApiMovies.Repositories;

namespace MinimalApiMovies.Validations
{
    public class CreateActorDTOValidator: AbstractValidator<CreateActorDTO>
    {
        public CreateActorDTOValidator(IActorRepository actorRepository,IHttpContextAccessor httpContextAccessor)
        {
            var routeValueId = httpContextAccessor.HttpContext?.GetRouteValue("id");
            var id = 0;

            if (routeValueId is string routeValueIdString)
            {
                int.TryParse(routeValueIdString, out id);
            }

            RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
            .MaximumLength(75).WithMessage(ValidationUtilities.MessageMaxLength)
            .Must(ValidationUtilities.FirstLetterIsUppercase).WithMessage(ValidationUtilities.UpperCaseMessage);

            var minimumDate = new DateTime(1900, 1, 1);

            RuleFor(p => p.BirthDate).GreaterThanOrEqualTo(minimumDate)
                .WithMessage(ValidationUtilities.GreaterThanDate(minimumDate));

            RuleFor(x => x.LastName)
                        .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
                        .MaximumLength(75).WithMessage(ValidationUtilities.MessageMaxLength)
                        .Must(ValidationUtilities.FirstLetterIsUppercase).WithMessage(ValidationUtilities.UpperCaseMessage);

            RuleFor(x => x)
                        .MustAsync(async (actor, _) =>
                        {
                            return !await actorRepository.Exists(
                                id,
                                $"{actor.FirstName} {actor.LastName}"
                            );
                        })
                        .WithMessage("Actor already exists.");

             

        }
    }
    
}
