using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using MinimalApiMovies.DTOs;
using MinimalApiMovies.Entities;
using MinimalApiMovies.Repositories;

namespace MinimalApiMovies.Endpoints
{
    public static class ActorEndpoints
    {
        public static RouteGroupBuilder MapActorEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllActors);
            group.MapGet("/{id:int}", GetActorById);
            group.MapPost("/", CreateActores);
            group.MapPut("/{id:int}", UpdateActor);
            group.MapDelete("/{id:int}", DeleteActor);
            return group;
        }

        static async Task<Ok<List<Actor>>> GetAllActors(IActorRepository repository)
        {
            var actors = await repository.GetAll();
            return TypedResults.Ok(actors);
        }

        static async Task<Results<Ok<Actor>, NotFound>> GetActorById(int id, IActorRepository repository)
        {
            var actor = await repository.GetById(id);
            if (actor == null)
            {
                return TypedResults.NotFound();
            }
            return TypedResults.Ok(actor);
        }

        static async Task<Results<Created<Actor>, NotFound>> CreateActores(CreateActorDTO createActorDTO, IActorRepository repository, IMapper mapper)
        {
            var actor = mapper.Map<Actor>(createActorDTO);
            var id = await repository.Create(actor);
            actor.Id = id;
            return TypedResults.Created($"/Actor/{id}", actor);
        }

        static async Task<Results<Ok, NotFound>> UpdateActor(int id, CreateActorDTO updateActorDTO, IActorRepository repository, IMapper mapper)
        {
            var exists = await repository.Exists(id);
            if (!exists)
            {
                return TypedResults.NotFound();
            }

            var actor = mapper.Map<Actor>(updateActorDTO);
            actor.Id = id;

            await repository.Update(actor);
            return TypedResults.Ok();
        }

        static async Task<Results<Ok, NotFound>> DeleteActor(int id, IActorRepository repository)
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
