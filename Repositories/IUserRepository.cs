using Microsoft.AspNetCore.Identity;
using MinimalApiMovies.Entities;
using System.Security.Claims;

namespace MinimalApiMovies.Repositories
{
    public interface IUserRepository
    {
        Task AssignClaims(IdentityUser user, IEnumerable<Claim> claims);
        Task<String> Create(IdentityUser user);
        //Task<List<User>> GetAll();
        //Task<User?> GetById(int id);

        //Task<bool> Exists(int id);
        //Task Update(User user);
        //Task Delete(int id);
        Task<IdentityUser?> GetByEmail(string normalizedEmail);
        Task<IList<Claim>> GetClaims(IdentityUser user);
        Task RemoveClaims(IdentityUser user, IEnumerable<Claim> claims);
    }
}
