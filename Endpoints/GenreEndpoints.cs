using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using MinimalApiMovies.DTOs;
using MinimalApiMovies.Entities;
using MinimalApiMovies.Fitlers;
using MinimalApiMovies.Repositories;

namespace MinimalApiMovies.Endpoints
{
    public static class GenreEndpoints
    {
        public static async Task<RouteGroupBuilder> MapGenreEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllGenres).RequireAuthorization();
            group.MapGet("/{id:int}", GetGenreById);
            group.MapPost("/", CreateGenre).RequireAuthorization("isadmin").AddEndpointFilter<ValidationFilter<CreateGenreDTO>>();
            group.MapPut("/{id:int}", UpdateGenre).RequireAuthorization("isadmin").AddEndpointFilter<ValidationFilter<CreateGenreDTO>>();
            group.MapDelete("/{id:int}", DeleteGenre).RequireAuthorization("isadmin");
            return group;
        }

        [OutputCache(Duration = 60)]
        static async Task<Ok<List<GenreDTO>>> GetAllGenres(IGenreRepository repository, IMapper mapper,ILoggerFactory loggerFactory)
        {


            var type = typeof(GenreEndpoints);
            var logger = loggerFactory.CreateLogger(type.FullName!);


            logger.LogTrace("This is a trace message");
            logger.LogDebug("This is a debug message");
            logger.LogInformation("This is a information message");
            logger.LogWarning("This is a warning message");
            logger.LogError("This is a error message");
            logger.LogCritical("This is a critical message");
            //logger.LogInformation("Getting the list of genres");

            var genres = await repository.GetAll();
            var genreDTOs = mapper.Map<List<GenreDTO>>(genres);
            return TypedResults.Ok(genreDTOs);
        }

        [OutputCache(Duration = 60)]
        static async Task<Results<Ok<GenreDTO>, NotFound>> GetGenreById(
            [AsParameters] GetGenreByIdRequestDTO model)
        {
            var genre = await model.Repository.GetById(model.id);
            if (genre == null)
            {
                return TypedResults.NotFound();
            }
            var genreDTO = model.Mapper.Map<GenreDTO>(genre);
            return TypedResults.Ok(genreDTO);
        }   

        static async Task<Results<Created<GenreDTO>,ValidationProblem >>CreateGenre(CreateGenreDTO createGenreDTO,
            [AsParameters] CreateGenreRequestDTO model)
        {
            var genre = model.Mapper.Map<Genre>(createGenreDTO);
            var id = await model.GenresRepository.Create(genre);
            genre.Id = id;
            var genreDTO = model.Mapper.Map<GenreDTO>(genre);
            return TypedResults.Created($"/Genre/{id}", genreDTO);
        }

        static async Task<Results<Ok, NotFound>> UpdateGenre(int id, CreateGenreDTO createGenreDTO,
            IGenreRepository repository, IMapper mapper,IValidator<CreateGenreDTO> validator)
        {
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
