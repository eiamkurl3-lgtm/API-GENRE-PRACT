using MinimalApiMovies.Entities;
using MinimalAPIsMovies.DTOs;

namespace MinimalAPIsMovies.Repositories
{
    public interface IMoviesRepository
    {
        //Task Assign(int id, List<int> genresIds);
        Task<int> Create(Movies movie);
        Task Delete(int id);
        Task<bool> Exist(int id);
        Task<List<Movies>> GetAll(PaginationDTO paginationDTO);
        Task<Movies?> GetbyId(int id);
        Task Update(Movies movie);
    }
}