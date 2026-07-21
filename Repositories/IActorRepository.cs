using MinimalApiMovies.Entities;
using MinimalAPIsMovies.DTOs;

namespace MinimalApiMovies.Repositories
{
    public interface IActorRepository
    {
        Task<int> Create(Actor actor);
        Task<List<Actor>> GetAll(PaginationDTO pagination);
        Task<Actor?> GetById(int id);

        Task<bool> Exists(int id);
        Task Update(Actor actor);
        Task Delete(int id);
        Task<List<Actor>> GetByName(string name);
        Task<List<int>> Exists(List<int> ids);
    }
}
