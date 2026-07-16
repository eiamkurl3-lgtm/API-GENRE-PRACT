using Dapper;
using Microsoft.Data.SqlClient;
using MinimalApiMovies.Entities;
using System.Data;

namespace MinimalApiMovies.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string connectionString;

        public UserRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<int> Create(User user)
        {
            using var connection = new SqlConnection(connectionString);

            var id = await connection.QuerySingleAsync<int>(
                "Create_User",
                new
                {
                    user.FirstName,
                    user.LastName,
                    user.Age,
                    user.Name,
                    user.Gender,
                    user.IsActive,
                    user.PhoneNumber,
                    user.Email
                },
                commandType: CommandType.StoredProcedure
            );

            user.Id = id;
            return id;
        }

        public async Task Delete(int id)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync(
                "User_Delete",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<bool> Exists(int id)
        {
            using var connection = new SqlConnection(connectionString);

            var exists = await connection.QuerySingleAsync<bool>(
                "User_IfExists",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );
            return exists;
        }

        public async Task<List<User>> GetAll()
        {
            using var connection = new SqlConnection(connectionString);

            var users = await connection.QueryAsync<User>(
                "User_GetAll",
                commandType: CommandType.StoredProcedure
            );

            return users.ToList();
        }

        public async Task<User?> GetById(int id)
        {
            using var connection = new SqlConnection(connectionString);

            var user = await connection.QueryFirstOrDefaultAsync<User>(
                "User_GetByID",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );

            return user;
        }

        public async Task Update(User user)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync(
                "Update_User",
                new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Age,
                    user.Name,
                    user.Gender,
                    user.IsActive,
                    user.PhoneNumber,
                    user.Email
                },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
