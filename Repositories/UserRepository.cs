using Dapper;
using Microsoft.AspNetCore.Identity;
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

        public async Task<IdentityUser?> GetByEmail(string normalizedEmail)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                return await connection
                    .QuerySingleOrDefaultAsync<IdentityUser>("Users_GetByEmail", new { normalizedEmail },
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<string> Create(IdentityUser user)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                user.Id = Guid.NewGuid().ToString();
                await connection.ExecuteAsync("Create_User",
                    new
                    {
                        user.Id,
                        user.Email,
                        user.NormalizedEmail,
                        user.UserName,
                        user.NormalizedUserName,
                        user.PasswordHash
                    }, commandType: CommandType.StoredProcedure);

                return user.Id;
            }
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
