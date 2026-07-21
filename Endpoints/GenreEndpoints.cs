using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using MinimalApiMovies.DTOs;
using MinimalApiMovies.Entities;
using MinimalApiMovies.Repositories;

namespace MinimalApiMovies.Endpoints
{
    public static class GenreEndpoints
    {
        public static RouteGroupBuilder MapGenreEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllGenres);
            group.MapGet("/{id:int}", GetGenreById);
            group.MapPost("/", CreateGenre);
            group.MapPut("/{id:int}", UpdateGenre);
            group.MapDelete("/{id:int}", DeleteGenre);
            return group;
        }

        [OutputCache(Duration = 60)]
        static async Task<Ok<List<GenreDTO>>> GetAllGenres(IGenreRepository repository, IMapper mapper)
        {
            var genres = await repository.GetAll();
            var genreDTOs = mapper.Map<List<GenreDTO>>(genres);
            return TypedResults.Ok(genreDTOs);
        }

        [OutputCache(Duration = 60)]
        static async Task<Results<Ok<GenreDTO>, NotFound>> GetGenreById(int id, IGenreRepository repository, IMapper mapper)
        {
            var genre = await repository.GetById(id);
            if (genre == null)
            {
                return TypedResults.NotFound();
            }
            var genreDTO = mapper.Map<GenreDTO>(genre);
            return TypedResults.Ok(genreDTO);
        }

        static async Task<Results<Created<GenreDTO>,ValidationProblem>> CreateGenre(CreateGenreDTO createGenreDTO, IGenreRepository repository, IMapper mapper,IValidator<CreateGenreDTO> validator)
        {
            var validationResult = await validator.ValidateAsync(createGenreDTO);

            if (!validationResult.IsValid)
            {
                return TypedResults.ValidationProblem(validationResult.ToDictionary());
            }


            var genre = mapper.Map<Genre>(createGenreDTO);
            var id = await repository.Create(genre);
            genre.Id = id;
            var genreDTO = mapper.Map<GenreDTO>(genre);
            return TypedResults.Created($"/Genre/{id}", genreDTO);
        }

        static async Task<Results<Ok, NotFound, ValidationProblem>> UpdateGenre(int id, CreateGenreDTO createGenreDTO,
            IGenreRepository repository, IMapper mapper,IValidator<CreateGenreDTO> validator)
        {
            var validationResult = await validator.ValidateAsync(createGenreDTO);

            if (!validationResult.IsValid)
            {
                return TypedResults.ValidationProblem(validationResult.ToDictionary());
            }

            var exists = await repository.Exists(id);
            if (!exists)
            {
                return TypedResults.NotFound();
            }

            var genre = mapper.Map<Genre>(createGenreDTO);
            genre.Id = id;

            await repository.Update(genre);
            return TypedResults.Ok();
        }

        static async Task<Results<Ok, NotFound>> DeleteGenre(int id, IGenreRepository repository)
        {
            var exists = await repository.Exists(id);
            if (!exists)
            {
                return TypedResults.NotFound();
            }
            await repository.Delete(id);
            return TypedResults.Ok();
        }
    }
}
