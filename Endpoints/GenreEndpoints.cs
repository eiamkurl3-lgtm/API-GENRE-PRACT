using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
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

        static async Task<Ok<List<Genre>>> GetAllGenres(IGenreRepository repository)
        {
            var genres = await repository.GetAll();
            return TypedResults.Ok(genres);
        }
        static async Task<Results<Ok<Genre>, NotFound>> GetGenreById(int id, IGenreRepository repository)
        {
            var genre = await repository.GetById(id);
            if (genre == null)
            {
                return TypedResults.NotFound();
            }
            return TypedResults.Ok(genre);
        }
        static async Task<Results<Created<Genre>, NotFound>> CreateGenre(CreateGenreDTO createGenreDTO, IGenreRepository repository, IMapper mapper)
        {
            var genre = mapper.Map<Genre>(createGenreDTO);
            var id = await repository.Create(genre);
            return TypedResults.Created($"/Genre/{id}", genre);
        }
        static async Task<Results<Ok, NotFound>> UpdateGenre(int id, CreateGenreDTO createGenreDTO, IGenreRepository repository, IMapper mapper)
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
