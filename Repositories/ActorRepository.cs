using Dapper;
using Microsoft.Data.SqlClient;
using MinimalApiMovies.Entities;
using MinimalAPIsMovies.DTOs;
using System.Data;

namespace MinimalApiMovies.Repositories
{
    public class ActorRepository : IActorRepository
    {
        private readonly string connectionString;
        private readonly HttpContext httpContext;

        public ActorRepository(IConfiguration configuration,IHttpContextAccessor httpContextAccessor)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection")!;
            httpContext = httpContextAccessor.HttpContext!;
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
                    //actor.Name
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

        public async Task<bool> Exists(int id, string name)
        {
            var parts = name.Split(' ', 2);
            var firstName = parts.Length > 0 ? parts[0] : name;
            var lastName = parts.Length > 1 ? parts[1] : string.Empty;

            using var connection = new SqlConnection(connectionString);
            var exists = await connection.QuerySingleAsync<bool>(
                "Actor_IfExistsByName",
                new { Id = id, FirstName = firstName, LastName = lastName },
                commandType: CommandType.StoredProcedure
            );
            return exists;
        }

        public async Task<List<Actor>> GetAll(PaginationDTO pagination)
        {
            using var connection = new SqlConnection(connectionString);

            var actors = await connection.QueryAsync<Actor>(
                "Actor_GetAll",
                new { pagination.Page, pagination.RecordsPerPage },
                commandType: CommandType.StoredProcedure);
                
                var actorsCount = await connection.QuerySingleAsync<int>("Actors_Count", commandType:CommandType.StoredProcedure);


            httpContext.Response.Headers.Append("totalAmountOfRecords", actorsCount.ToString());

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
        //GetByName

        public async Task<List<Actor>> GetByName(string name)
        {
            using var connection = new SqlConnection(connectionString);
            var actors = await connection.QueryAsync<Actor>(
                "Actors_GetByName",
                new { Name = name },
                commandType: CommandType.StoredProcedure
            );
            return actors.ToList();
        }

        public async Task<List<int>> Exists(List<int> ids)
        {
            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(int));

            foreach (var id in ids)
            {
                dt.Rows.Add(id);
            }

            using (var connection = new SqlConnection(connectionString))
            {
                var idsOfExistingActors = await connection.QueryAsync<int>
                    ("Actors_GetBySeveralIds", new { actorsIds = dt },
                    commandType: CommandType.StoredProcedure);
                return idsOfExistingActors.ToList();
            }
        }
    }
}
