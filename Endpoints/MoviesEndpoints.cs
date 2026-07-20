using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MinimalApiMovies.DTOs;
using MinimalApiMovies.Entities;
using MinimalApiMovies.Repositories;
using MinimalApiMovies.Services;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Repositories;

namespace MinimalApiMovies.Endpoints
{
    public static class MoviesEndpoints
    {
        private readonly static string container = "movies";

        public static RouteGroupBuilder MapMoviesEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllMovies).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("movies-get"));
            group.MapGet("/{id:int}", GetMovieById);
            group.MapPost("/", CreateMovie).DisableAntiforgery();
            group.MapPut("/{id:int}", UpdateMovie).DisableAntiforgery();
            group.MapDelete("/{id:int}", DeleteMovie);
            return group;
        }

        [OutputCache(Duration = 60)]
        static async Task<Ok<List<MoviesDTO>>> GetAllMovies(IMoviesRepository repository, IMapper mapper, int page = 1, int recordsPerPage = 10)
        {
            var paginationDTO = new PaginationDTO() { Page = page, RecordsPerPage = recordsPerPage };
            var movies = await repository.GetAll(paginationDTO);
            var moviesDTOs = mapper.Map<List<MoviesDTO>>(movies);
            return TypedResults.Ok(moviesDTOs);
        }

        [OutputCache(Duration = 60)]
        static async Task<Results<Ok<MoviesDTO>, NotFound>> GetMovieById(int id, IMoviesRepository repository, IMapper mapper)
        {
            var movie = await repository.GetbyId(id);
            if (movie == null)
            {
                return TypedResults.NotFound();
            }
            var movieDTO = mapper.Map<MoviesDTO>(movie);
            return TypedResults.Ok(movieDTO);
        }

        static async Task<Created<MoviesDTO>> CreateMovie([FromForm] CreateMoviesDTO createMoviesDTO,
            IOutputCacheStore outputCacheStore, IMoviesRepository repository,
            IMapper mapper, IFileStorage fileStorage)
        {
            var movie = mapper.Map<Movies>(createMoviesDTO);

            if (createMoviesDTO.Poster is not null)
            {
                var url = await fileStorage.Store(container, createMoviesDTO.Poster);
                movie.Poster = url;
            }

            var id = await repository.Create(movie);
            await outputCacheStore.EvictByTagAsync("movies-get", default);
            var movieDTO = mapper.Map<MoviesDTO>(movie);
            return TypedResults.Created($"/Movies/{id}", movieDTO);
        }

        static async Task<Results<NoContent, NotFound>> UpdateMovie(int id,
            [FromForm] CreateMoviesDTO updateMoviesDTO, IMoviesRepository repository,
            IFileStorage fileStorage, IOutputCacheStore outputCacheStore,
            IMapper mapper)
        {
            var movieDB = await repository.GetbyId(id);

            if (movieDB is null)
            {
                return TypedResults.NotFound();
            }

            var movieForUpdate = mapper.Map<Movies>(updateMoviesDTO);
            movieForUpdate.Id = id;
            movieForUpdate.Poster = movieDB.Poster;

            if (updateMoviesDTO.Poster is not null)
            {
                var url = await fileStorage.Edit(movieForUpdate.Poster,
                    container, updateMoviesDTO.Poster);
                movieForUpdate.Poster = url;
            }

            await repository.Update(movieForUpdate);
            await outputCacheStore.EvictByTagAsync("movies-get", default);
            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound>> DeleteMovie(int id,
            IMoviesRepository repository, IOutputCacheStore outputCacheStore,
            IFileStorage fileStorage)
        {
            var movieDB = await repository.GetbyId(id);

            if (movieDB is null)
            {
                return TypedResults.NotFound();
            }

            await repository.Delete(id);
            await fileStorage.Delete(movieDB.Poster, container);
            await outputCacheStore.EvictByTagAsync("movies-get", default);
            return TypedResults.NoContent();
        }
    }
}
