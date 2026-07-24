using Microsoft.AspNetCore.Identity;

namespace MinimalApiMovies.Services
{
    public interface IUsersService
    {
        Task<IdentityUser?> GetUser();
    }
}