using Dapper;
using Microsoft.Data.SqlClient;
using MinimalApiMovies.Entities;

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

            var query = @"
                INSERT INTO Users 
                (FirstName, LastName, Age, Name, Gender, IsActive, PhoneNumber, Email)
                VALUES 
                (@FirstName, @LastName, @Age, @Name, @Gender, @IsActive, @PhoneNumber, @Email);

                SELECT SCOPE_IDENTITY();
            ";

            var id = await connection.QuerySingleAsync<int>(query, user);

            user.Id = id;

            return id;
        }

        public async Task Delete(int id)
        {
            using var connection = new SqlConnection(connectionString);

            var query = @"DELETE FROM Users WHERE Id = @Id";

            await connection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<bool> Exists(int id)
        {
            using var connection = new SqlConnection(connectionString);

            var query = @"
                IF EXISTS (SELECT 1 FROM Users WHERE Id = @Id)
                    SELECT 1;
                ELSE
                    SELECT 0;
            ";

            return await connection.QuerySingleAsync<bool>(query, new { Id = id });
        }

        public async Task<List<User>> GetAll()
        {
            using var connection = new SqlConnection(connectionString);

            var users = await connection.QueryAsync<User>(
                "SELECT * FROM Users ORDER BY FirstName"
            );

            return users.ToList();
        }

        public async Task<User?> GetById(int id)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QuerySingleOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Id = @Id",
                new { Id = id }
            );
        }

        public async Task Update(User user)
        {
            using var connection = new SqlConnection(connectionString);

            var query = @"
                UPDATE Users 
                SET 
                    FirstName = @FirstName,
                    LastName = @LastName,
                    Age = @Age,
                    Name = @Name,
                    Gender = @Gender,
                    IsActive = @IsActive,
                    PhoneNumber = @PhoneNumber,
                    Email = @Email
                WHERE Id = @Id;
            ";

            await connection.ExecuteAsync(query, user);
        }
    }
}