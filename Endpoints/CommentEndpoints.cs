using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MinimalApiMovies.DTOs;
using MinimalApiMovies.Entities;
using MinimalApiMovies.Fitlers;
using MinimalApiMovies.Repositories;
using MinimalApiMovies.Services;
using MinimalAPIsMovies.DTOs;
using MinimalAPIsMovies.Repositories;
using System.Xml.Linq;

namespace MinimalApiMovies.Endpoints
{
    public static class CommentEndpoints
    {
        public static RouteGroupBuilder MapCommentEndpoints(this RouteGroupBuilder  group)
        {
            group.MapGet("/", GetAll)
              .CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("comments-get"));
            group.MapGet("/{id:int}", GetById).WithName("GetCommentById");
            group.MapPost("/", Create).RequireAuthorization().AddEndpointFilter<ValidationFilter<CreateCommentsDTO>>();

            group.MapPut("/{id:int}", Update).RequireAuthorization().AddEndpointFilter<ValidationFilter<CreateCommentsDTO>>();
            group.MapDelete("/{id:int}", Delete).RequireAuthorization();
            return group;
        }

        static async Task<Results<Ok<List<CommentsDTO>>, NotFound>> GetAll(int movieId,
            ICommentRepository commentsRepository, IMoviesRepository moviesRepository,
            IMapper mapper)
        {
            if (!await moviesRepository.Exists(movieId))
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
            if (!await moviesRepository.Exists(movieId))
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

        static async Task<Results<CreatedAtRoute<CommentsDTO>, NotFound, BadRequest<string>>> Create(int movieId,
            CreateCommentsDTO createCommentDTO, ICommentRepository commentsRepository,
            IMoviesRepository moviesRepository, IMapper mapper,
            IOutputCacheStore outputCacheStore,IUsersService usersService)
        {
            if (!await moviesRepository.Exists(movieId))
            {
                return TypedResults.NotFound();
            }

            var user = await usersService.GetUser();

            if(user is null)
            {
                return TypedResults.BadRequest("User Not Found");
            }

            var comment = mapper.Map<Comments>(createCommentDTO);
            comment.MovieId = movieId;
            comment.UserId= user.Id;
            var id = await commentsRepository.Create(comment);
            await outputCacheStore.EvictByTagAsync("comments-get", default);
            var commentDTO = mapper.Map<CommentsDTO>(comment);
            return TypedResults.CreatedAtRoute(commentDTO, "GetCommentById", new { id, movieId });
        }

        static async Task<Results<NoContent, NotFound, ForbidHttpResult>> Update(int movieId,
            int id, CreateCommentsDTO createCommentDTO, IOutputCacheStore outputCacheStore,
            ICommentRepository commentsRepository, IMoviesRepository moviesRepository,
            IMapper mapper, IUsersService usersService)
        {
            if (!await moviesRepository.Exists(movieId))
            {
                return TypedResults.NotFound();
            }

            var commentFromDB = await commentsRepository.GetById(id);

            if (commentFromDB is null)
            {
                return TypedResults.NotFound();
            }

            var user = await usersService.GetUser();

            if (user is null)
            {
                return TypedResults.NotFound();
            }

            if (commentFromDB.UserId != user.Id)
            {
                return TypedResults.Forbid();
            }

            commentFromDB.Body = createCommentDTO.Body;

            await commentsRepository.Update(commentFromDB);
            await outputCacheStore.EvictByTagAsync("comments-get", default);
            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound, ForbidHttpResult>>
            Delete(int movieId, int id,
            ICommentRepository commentsRepository, IMoviesRepository moviesRepository,
            IOutputCacheStore outputCacheStore, IUsersService usersService)
        {
            if (!await moviesRepository.Exists(movieId))
            {
                return TypedResults.NotFound();
            }

            var commentFromDB = await commentsRepository.GetById(id);

            if (commentFromDB is null)
            {
                return TypedResults.NotFound();
            }

            var user = await usersService.GetUser();

            if (user is null)
            {
                return TypedResults.NotFound();
            }

            if (commentFromDB.UserId != user.Id)
            {
                return TypedResults.Forbid();
            }

            await commentsRepository.Delete(id);
            await outputCacheStore.EvictByTagAsync("comments-get", default);
            return TypedResults.NoContent();
        }
    }
}