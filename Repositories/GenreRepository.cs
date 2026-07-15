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

        public async Task<List<Genre>> GetAll()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var genres = await connection.QueryAsync<Genre>(
                    @"SELECT Id, Name, Description
              FROM Genre");

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
    }
}
