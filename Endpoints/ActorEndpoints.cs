using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using MinimalApiMovies.DTOs;
using MinimalApiMovies.Entities;
using MinimalApiMovies.Fitlers;
using MinimalApiMovies.Repositories;
using MinimalApiMovies.Services;
using MinimalAPIsMovies.DTOs;
using System.ComponentModel.DataAnnotations;

namespace MinimalApiMovies.Endpoints
{
    public static class ActorEndpoints
    {
        private readonly static string container = "actors";
        public static RouteGroupBuilder MapActorEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllActors);
            group.MapGet("/{id:int}", GetActorById);
            group.MapPost("/", CreateActores).RequireAuthorization("isadmin").DisableAntiforgery().AddEndpointFilter<ValidationFilter<CreateActorDTO>>();
            group.MapPut("/{id:int}", UpdateActor).RequireAuthorization("isadmin").DisableAntiforgery().AddEndpointFilter<ValidationFilter<CreateActorDTO>>();
            group.MapDelete("/{id:int}", DeleteActor).RequireAuthorization("isadmin");
            group.MapGet("/search/{name}", GetActorsByName);
            return group;
        }

        [OutputCache(Duration = 60)]
        static async Task<Ok<List<ActorDTO>>> GetAllActors(IActorRepository repository, IMapper mapper, int page = 1, int recordsPerPage= 10)
        {
            var paginationDTO = new PaginationDTO() { Page = page, RecordsPerPage = recordsPerPage };
            var actors = await repository.GetAll(paginationDTO);
            var actorDTOs = mapper.Map<List<ActorDTO>>(actors);
            return TypedResults.Ok(actorDTOs);
        }

        [OutputCache(Duration = 60)]
        static async Task<Results<Ok<ActorDTO>, NotFound>> GetActorById(int id, IActorRepository repository, IMapper mapper)
        {
            var actor = await repository.GetById(id);
            if (actor == null)
            {
                return TypedResults.NotFound();
            }
            var actorDTO = mapper.Map<ActorDTO>(actor);
            return TypedResults.Ok(actorDTO);
        }

        static async Task<Created<ActorDTO>> CreateActores([FromForm] CreateActorDTO createActorDTO,
            IOutputCacheStore outputCacheStore ,IActorRepository repository, 
            IMapper mapper, IFileStorage fileStorage)
        {
            var actor = mapper.Map<Actor>(createActorDTO);

            if(createActorDTO.ProfilePicture is not null)
            {
                var url = await fileStorage.Store(container, createActorDTO.ProfilePicture);
                actor.ProfilePicture = url;
            }
            var id = await repository.Create(actor);
            await outputCacheStore.EvictByTagAsync("actors-get)", default);
            var actorDTO = mapper.Map<ActorDTO>(actor);
            return TypedResults.Created($"/Actor/{id}", actorDTO);
        }

        static async Task<Results<NoContent, NotFound>> UpdateActor(int id,
            [FromForm] CreateActorDTO createActorDTO, IActorRepository repository,
            IFileStorage fileStorage, IOutputCacheStore outputCacheStore,
            IMapper mapper)
        {
            var actorDB = await repository.GetById(id);

            if (actorDB is null)
            {
                return TypedResults.NotFound();
            }

            var actorForUpdate = mapper.Map<Actor>(createActorDTO);
            actorForUpdate.Id = id;
            actorForUpdate.ProfilePicture = actorDB.ProfilePicture;

            if (createActorDTO.ProfilePicture is not null)
            {
                var url = await fileStorage.Edit(actorForUpdate.ProfilePicture,
                    container, createActorDTO.ProfilePicture);
                actorForUpdate.ProfilePicture = url;
            }

            await repository.Update(actorForUpdate);
            await outputCacheStore.EvictByTagAsync("actors-get", default);
            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound>> DeleteActor(int id,
            IActorRepository repository, IOutputCacheStore outputCacheStore,
            IFileStorage fileStorage)
        {
            var actorDB = await repository.GetById(id);

            if (actorDB is null)
            {
                return TypedResults.NotFound();
            }

            await repository.Delete(id);
            await fileStorage.Delete(actorDB.ProfilePicture, container);
            await outputCacheStore.EvictByTagAsync("actors-get", default);
            return TypedResults.NoContent();
        }

        //getbyname
        public async static Task<Ok<List<ActorDTO>>> GetActorsByName(string name, IActorRepository repository, IMapper mapper)
        {
            var actors = await repository.GetByName(name);
            var actorDTOs = mapper.Map<List<ActorDTO>>(actors);
            return TypedResults.Ok(actorDTOs);
        }
    }
}
