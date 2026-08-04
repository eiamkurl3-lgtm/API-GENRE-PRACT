using Dapper;
using Microsoft.Data.SqlClient;
using MinimalApiMovies.Entities;
using System.Data;

namespace MinimalApiMovies.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly string connectionString;

        public RefreshTokenRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task CreateAsync(RefreshToken refreshToken)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                "Create_RefreshToken",
                new
                {
                    refreshToken.Id,
                    refreshToken.UserId,
                    refreshToken.TokenHash,
                    refreshToken.ExpiresAt
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QuerySingleOrDefaultAsync<RefreshToken>(
                "Get_RefreshToken",
                new { TokenHash = tokenHash },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task RevokeAsync(Guid id, Guid? replacedByTokenId = null)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                "Revoke_RefreshToken",
                new
                {
                    Id = id,
                    RevokedAt = DateTime.UtcNow,
                    ReplacedByTokenId = replacedByTokenId
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task RevokeAllForUserAsync(string userId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                "Revoke_AllUserRefreshTokens",
                new
                {
                    UserId = userId,
                    RevokedAt = DateTime.UtcNow
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task CleanupForUserAsync(string userId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(
                "Cleanup_ExpiredRefreshTokens",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
