using MinimalApiMovies.Entities;

namespace MinimalApiMovies.Repositories
{
    public interface IUserRepository
    {
        Task<int> Create(User user);
        Task<List<User>> GetAll();
        Task<User?> GetById(int id);

        Task<bool> Exists(int id);
        Task Update(User user);
        Task Delete(int id);
    }
}
