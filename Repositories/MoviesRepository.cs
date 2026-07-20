using Dapper;
using Microsoft.Data.SqlClient;
using MinimalApiMovies.Entities;
using MinimalAPIsMovies.DTOs;
using MinimalApiMovies.Entities;
using System.Data;
using System.Xml.Linq;

namespace MinimalAPIsMovies.Repositories
{
    public class MoviesRepository : IMoviesRepository
    {
        private readonly string connectionString;
        private readonly HttpContext httpContext;

        public MoviesRepository(IConfiguration configuration, IHttpContextAccessor
            httpContextAccessor)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection")!;
            httpContext = httpContextAccessor.HttpContext!;
        }

        public async Task<int> Create(Movies movie)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var id = await connection.QuerySingleAsync<int>("Movies_Create",
                    new { movie.Title, movie.Poster, movie.ReleaseDate, movie.InTheaters },
                    commandType: CommandType.StoredProcedure);
                movie.Id = id;
                return id;
            }
        }

        public async Task<List<Movies>> GetAll(PaginationDTO paginationDTO)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var movies = await connection.QueryAsync<Movies>("Movies_GetAll",
                    new { paginationDTO.Page, paginationDTO.RecordsPerPage },
                    commandType: CommandType.StoredProcedure);

                var moviesCount = await connection.QuerySingleAsync<int>("Movies_Count",
                    commandType: CommandType.StoredProcedure);

                httpContext.Response.Headers.Append("totalAmountOfRecords",
                    moviesCount.ToString());

                return movies.ToList();
            }
        }

        public async Task<Movies?> GetbyId(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var movie = await connection.QueryFirstOrDefaultAsync<Movies>("Movies_GetById",
                    new { id },
                    commandType: CommandType.StoredProcedure);
                return movie;
            }
        }

        public async Task<bool> Exist(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var exist = await connection.QueryFirstOrDefaultAsync<int>("Movies_Exist",
                    new { id },
                    commandType: CommandType.StoredProcedure);
                return exist == 1;
            }
        }

        public async Task Update(Movies movie)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync("Movies_Update",
                    new { movie.Id, movie.Title, movie.Poster, movie.ReleaseDate, movie.InTheaters },
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task Delete(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync("Movies_Delete",
                    new { id },
                    commandType: CommandType.StoredProcedure);
            }
        }

        //    public Task Assign(int id, List<int> genresIds)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public Task<bool> Exists(int id)
        //    {
        //        throw new NotImplementedException();
        //    }
    }
}
