using MinimalApiMovies.Entities;

namespace MinimalApiMovies.Repositories
{
    public interface ICommentRepository
    {
        Task<int> Create(Comments comment);
        Task Delete(int id);
        Task<bool> Exists(int id);
        Task<List<Comments>> GetAll(int movieId);
        Task<Comments?> GetById(int id);
        Task Update(Comments comment);
    }
}