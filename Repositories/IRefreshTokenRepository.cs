using MinimalApiMovies.Entities;

namespace MinimalApiMovies.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task CreateAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
        Task RevokeAsync(Guid id, Guid? replacedByTokenId = null);
        Task RevokeAllForUserAsync(string userId);
        Task CleanupForUserAsync(string userId);
    }
}
