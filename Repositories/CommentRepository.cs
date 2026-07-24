using Dapper;
using Microsoft.Data.SqlClient;
using MinimalApiMovies.Entities;
using MinimalAPIsMovies.DTOs;
using System.Data;


namespace MinimalApiMovies.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly string connectionString;
        private readonly HttpContext httpContext;

        public CommentRepository(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection")!;
            httpContext = httpContextAccessor.HttpContext!;
        }

        public async Task<int> Create(Comments comment)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(
                "Create_Comment",
                new
                {
                    comment.Body,
                    comment.MovieId,
                    comment.UserId
                },
                commandType: CommandType.StoredProcedure
            );
            comment.id = id;
            return id;
        }

        public async Task<List<Comments>> GetAll(int movieId)
        {
            using var connection = new SqlConnection(connectionString);
            var comments = await connection.QueryAsync<Comments>(
                "Get_CommentsByMovieId",
                new { MovieId = movieId },
                commandType: CommandType.StoredProcedure
            );
            return comments.ToList();
        }

        public async Task Delete(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                "Delete_Comment",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<bool> Exists(int id)
        {
            using var connection = new SqlConnection(connectionString);
            var exists = await connection.QuerySingleAsync<bool>(
                "Comment_IfExists",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );
            return exists;
        }

        public async Task Update(Comments comment)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                "Update_Comment",
                new
                {
                    comment.id,
                    comment.Body,
                    comment.MovieId
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<Comments?> GetById(int id)
        {
            using var connection = new SqlConnection(connectionString);
            var comment = await connection.QueryFirstOrDefaultAsync<Comments>(
                "Comment_GetByID",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );
            return comment;
        }
    }
}
