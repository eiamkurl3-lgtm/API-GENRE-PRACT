using AutoMapper;
using MinimalApiMovies.Repositories;

namespace MinimalApiMovies.DTOs
{
    public class GetGenreByIdRequestDTO
    {
        public IGenreRepository Repository { get; set; } = null!;
        public int id { get; set; } 
        public IMapper Mapper { get; set; } = null!;
    }
}
