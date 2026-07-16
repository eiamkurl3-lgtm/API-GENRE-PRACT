using Dapper;
using Microsoft.Data.SqlClient;
using MinimalApiMovies.Entities;
using System.Data;

namespace MinimalApiMovies.Repositories
{
    public class GenreRepository : IGenreRepository
    {
        private readonly string connectionString;

        public GenreRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<int> Create(Genre genre)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
                    INSERT INTO Genre(Name,Description)
                    Values(@name,@Description);

                    SELECT SCOPE_IDENTITY()";

                var id = await connection.QuerySingleAsync<int>(query, genre);
                genre.Id = id;
                return id;
                
            }
       
        }

        public async Task<bool> Exists(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var Exists = await connection.QuerySingleAsync<bool>(
                    @"IF EXISTS (SELECT 1 FROM Genre WHERE Id = @Id)
                        SELECT 1;
                    ELSE
                        SELECT 0;",
                    new { id });
                return Exists;
            }
        }

        public async Task<List<Genre>> GetAll()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var genres = await connection.QueryAsync<Genre>(
                    @"SELECT Id, Name, Description
                        FROM Genre
                        ORDER BY Name");

                return genres.ToList();
            }
        }

        public async Task<Genre?> GetById(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var genre = await connection.QueryFirstOrDefaultAsync<Genre>(
                    @"SELECT Id, Name, Description
              FROM Genre
              WHERE Id = @Id",
                    new { Id = id });

                return genre;
            }
        }
            
        public async Task Update(Genre genre)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync(
                    @"UPDATE Genre
                        SET Name = @Name,
                            Description = @Description
                        WHERE Id = @Id",genre);
            }
        }

        public async Task Delete(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync(
                    @"DELETE FROM Genre
                        WHERE Id = @Id", new { Id = id });
            }
        }
    }
}
