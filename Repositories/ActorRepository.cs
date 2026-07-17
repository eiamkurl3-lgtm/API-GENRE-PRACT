using Dapper;
using Microsoft.Data.SqlClient;
using MinimalApiMovies.Entities;
using System.Data;

namespace MinimalApiMovies.Repositories
{
    public class ActorRepository : IActorRepository
    {
        private readonly string connectionString;

        public ActorRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<int> Create(Actor actor)
        {
            using var connection = new SqlConnection(connectionString);

            var id = await connection.QuerySingleAsync<int>(
                "Create_Actor",
                new
                {
                    actor.FirstName,
                    actor.LastName,
                    actor.BirthDate,
                    actor.ProfilePicture
                },
                commandType: CommandType.StoredProcedure
            );

            actor.Id = id;
            return id;
        }

        public async Task<bool> Exists(int id)
        {
            using var connection = new SqlConnection(connectionString);

            var exists = await connection.QuerySingleAsync<bool>(
                "Actor_IfExists",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );
            return exists;
        }

        public async Task<List<Actor>> GetAll()
        {
            using var connection = new SqlConnection(connectionString);

            var actors = await connection.QueryAsync<Actor>(
                "Actor_GetAll",
                commandType: CommandType.StoredProcedure
            );

            return actors.ToList();
        }

        public async Task<Actor?> GetById(int id)
        {
            using var connection = new SqlConnection(connectionString);

            var actor = await connection.QueryFirstOrDefaultAsync<Actor>(
                "Actor_GetByID",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );

            return actor;
        }

        public async Task Update(Actor actor)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync(
                "Update_Actor",
                new
                {
                    actor.Id,
                    actor.FirstName,
                    actor.LastName,
                    actor.BirthDate,
                    actor.ProfilePicture
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task Delete(int id)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync(
                "Actor_Delete",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
