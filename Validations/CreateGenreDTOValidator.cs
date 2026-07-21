using FluentValidation;
using MinimalApiMovies.DTOs;
using MinimalApiMovies.Repositories;

namespace MinimalApiMovies.Validations
{
    public class CreateGenreDTOValidator : AbstractValidator<CreateGenreDTO>
    {
        public CreateGenreDTOValidator(IGenreRepository genreRepository,IHttpContextAccessor httpContextAccessor)

        {
            var routeValueId= httpContextAccessor.HttpContext?.GetRouteValue("id");
            var id = 0;

            if (routeValueId is string routeValueIdString)
            {
                int.TryParse(routeValueIdString, out id);
            }

            RuleFor(p => p.Name)
                .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
                    .MaximumLength(150).
                        WithMessage(ValidationUtilities.MessageMaxLength)
                    .Must(ValidationUtilities.FirstLetterIsUppercase).WithMessage(ValidationUtilities.UpperCaseMessage)
                    .MustAsync(async (name, _) =>
                     {
                         var exists = await genreRepository.Exists(id , name);
                         return !exists;
                     }).WithMessage(g=> $"genre with the {g.Name} already exists");
        }
    }
}
