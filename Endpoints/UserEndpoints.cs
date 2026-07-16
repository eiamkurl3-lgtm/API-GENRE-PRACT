using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using MinimalApiMovies.DTOs;
using MinimalApiMovies.Entities;
using MinimalApiMovies.Repositories;

namespace MinimalApiMovies.Endpoints
{
    public static class UserEndpoints
    {

        public static RouteGroupBuilder MapUsersEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetAllUsers);
            group.MapGet("/{id:int}", GetUserById);
            group.MapPost("/", CreateUser);
            group.MapPut("/{id:int}", UpdateUser);
            group.MapDelete("/{id:int}", DeleteUser);
            return group;
        }
         
        static async Task<Ok<List<User>>> GetAllUsers(IUserRepository repository)
        {
            var users = await repository.GetAll();
            return TypedResults.Ok(users);
        }

        static async Task<Results<Ok<User>, NotFound>> GetUserById(int id, IUserRepository repository)
        {
            var user = await repository.GetById(id);
            if (user == null)
            {
                return TypedResults.NotFound();
            }
            return TypedResults.Ok(user);
        }

        static async Task<Results<Created<User>, NotFound>> CreateUser(CreateUserDTO createUserDTO, IUserRepository repository, IMapper mapper)
        {
            var user = mapper.Map<User>(createUserDTO);
            var id = await repository.Create(user);
            user.Id = id;
            return TypedResults.Created($"/User/{id}", user);
        }


        static async Task<Results<Ok, NotFound, NoContent>> UpdateUser(int id, CreateUserDTO updateUserDTO, IUserRepository repository, IMapper mapper)
        {
            var exists = await repository.Exists(id);
            if (!exists)
            {
                return TypedResults.NotFound();
            }
            var user = mapper.Map<User>(updateUserDTO);
            user.Id = id;
            await repository.Update(user);
            return TypedResults.Ok();
        }


        static async Task<Results<Ok, NotFound, NoContent>> DeleteUser(int id, IUserRepository repository)
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
