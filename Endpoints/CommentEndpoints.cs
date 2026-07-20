using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MinimalApiMovies.DTOs;
using MinimalApiMovies.Entities;
using MinimalApiMovies.Repositories;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Repositories;
using System.Xml.Linq;

namespace MinimalApiMovies.Endpoints
{
    public static class CommentEndpoints
    {
        private readonly static string container = "comments";

        public static RouteGroupBuilder MapCommentEndpoints(this RouteGroupBuilder  group)
        {
            group.MapGet("/", GetAll)
              .CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("comments-get"));
            group.MapGet("/{id:int}", GetById).WithName("GetCommentById");
            group.MapPost("/", Create);

            group.MapPut("/{id:int}", Update);
            group.MapDelete("/{id:int}", Delete);
            return group;
        }

        static async Task<Results<Ok<List<CommentsDTO>>, NotFound>> GetAll(int movieId,
            ICommentRepository commentsRepository, IMoviesRepository moviesRepository,
            IMapper mapper)
        {
            if (!await moviesRepository.Exist(movieId))
            {
                return TypedResults.NotFound();
            }

            var comments = await commentsRepository.GetAll(movieId);
            var commentsDTO = mapper.Map<List<CommentsDTO>>(comments);
            return TypedResults.Ok(commentsDTO);
        }

        static async Task<Results<Ok<CommentsDTO>, NotFound>> GetById(int movieId, int id,
            ICommentRepository commentsRepository, IMoviesRepository moviesRepository,
            IMapper mapper)
        {
            if (!await moviesRepository.Exist(movieId))
            {
                return TypedResults.NotFound();
            }

            var comment = await commentsRepository.GetById(id);

            if (comment is null)
            {
                return TypedResults.NotFound();
            }

            var commentDTO = mapper.Map<CommentsDTO>(comment);
            return TypedResults.Ok(commentDTO);
        }

        static async Task<Results<CreatedAtRoute<CommentsDTO>, NotFound>> Create(int movieId,
            CreateCommentsDTO createCommentDTO, ICommentRepository commentsRepository,
            IMoviesRepository moviesRepository, IMapper mapper,
            IOutputCacheStore outputCacheStore)
        {
            if (!await moviesRepository.Exist(movieId))
            {
                return TypedResults.NotFound();
            }

            var comment = mapper.Map<Comments>(createCommentDTO);
            comment.MovieId = movieId;
            var id = await commentsRepository.Create(comment);
            await outputCacheStore.EvictByTagAsync("comments-get", default);
            var commentDTO = mapper.Map<CommentsDTO>(comment);
            return TypedResults.CreatedAtRoute(commentDTO, "GetCommentById", new { id, movieId });
        }

        static async Task<Results<NoContent, NotFound>> Update(int movieId,
            int id, CreateCommentsDTO createCommentDTO, IOutputCacheStore outputCacheStore,
            ICommentRepository commentsRepository, IMoviesRepository moviesRepository,
            IMapper mapper)
        {
            if (!await moviesRepository.Exist(movieId))
            {
                return TypedResults.NotFound();
            }

            if (!await commentsRepository.Exists(id))
            {
                return TypedResults.NotFound();
            }

            var comment = mapper.Map<Comments>(createCommentDTO);
            comment.id = id;
            comment.MovieId = movieId;

            await commentsRepository.Update(comment);
            await outputCacheStore.EvictByTagAsync("comments-get", default);
            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound>> Delete(int movieId, int id,
            ICommentRepository commentsRepository, IMoviesRepository moviesRepository,
            IOutputCacheStore outputCacheStore)
        {
            if (!await moviesRepository.Exist(movieId))
            {
                return TypedResults.NotFound();
            }

            if (!await commentsRepository.Exists(id))
            {
                return TypedResults.NotFound();
            }

            await commentsRepository.Delete(id);
            await outputCacheStore.EvictByTagAsync("comments-get", default);
            return TypedResults.NoContent();
        }
    }
}