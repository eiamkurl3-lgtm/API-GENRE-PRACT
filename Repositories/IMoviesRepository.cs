using MinimalApiMovies.Entities;
using MinimalAPIsMovies.DTOs;

namespace MinimalAPIsMovies.Repositories
{
    public interface IMoviesRepository
    {
        Task Assign(int id, List<int> genresIds);
        Task Assign(int id, List<ActorMovie> actors);
        Task<int> Create(Movies movie);
        Task Delete(int id);
        Task<bool> Exists(int id);
        //Task<bool> Exists(int id);
        Task<List<Movies>> GetAll(PaginationDTO paginationDTO);
        Task<Movies?> GetById(int id);
        Task Update(Movies movie);
    }
}