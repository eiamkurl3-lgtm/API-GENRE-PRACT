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

        //public async Task<int> Create(Genre genre)
        //{
        //    using (var connection = new SqlConnection(connectionString))
        //    {
        //        var id = await connection.QuerySingleAsync<int>("Create_Genre",
        //            new {genre.Name});
        //        genre.Id = id;
        //        return id;

        //    }

        //}

        public async Task<int> Create(Genre genre)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var id = await connection.QuerySingleAsync<int>(
                    "Create_Genre",
                    new
                    {
                        name = genre.Name,
                        Description = genre.Description
                    },
                    commandType: CommandType.StoredProcedure
                );

                genre.Id = id;
                return id;
            }
        }

        public async Task<bool> Exists(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var Exists = await connection.QuerySingleAsync<bool>(
                    @"Genre_IfExists", new { id },commandType:CommandType.StoredProcedure);
                return Exists;
            }
        } 

        public async Task<List<Genre>> GetAll()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var genres = await connection.QueryAsync<Genre>
                    (@"Genre_GetAll",commandType:CommandType.StoredProcedure);

                return genres.ToList();
            }
        }

        public async Task<Genre?> GetById(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var genre = await connection.QueryFirstOrDefaultAsync<Genre>(
                    @"Genre_GetByID",
                    new { Id = id },commandType:CommandType.StoredProcedure);

                return genre;
            }
        }
            
        public async Task Update(Genre genre)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync(
                    @"Update_Genre",  new
                    {
                        id = genre.Id,
                        name = genre.Name,
                        Description = genre.Description
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public async Task Delete(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync(
                    @"Genre_Delete", new { Id = id },commandType:CommandType.StoredProcedure);
            }
        }
    }
}
