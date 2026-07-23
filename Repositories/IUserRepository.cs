using Microsoft.AspNetCore.Identity;
using MinimalApiMovies.Entities;

namespace MinimalApiMovies.Repositories
{
    public interface IUserRepository
    {
        Task<String> Create(IdentityUser user);
        Task<List<User>> GetAll();
        Task<User?> GetById(int id);

        Task<bool> Exists(int id);
        Task Update(User user);
        Task Delete(int id);
        Task<IdentityUser?> GetByEmail(string normalizedEmail);
    }
}
