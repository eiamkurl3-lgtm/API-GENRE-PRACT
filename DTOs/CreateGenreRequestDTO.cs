using AutoMapper;
using Microsoft.AspNetCore.OutputCaching;
using MinimalApiMovies.Repositories;

namespace MinimalApiMovies.DTOs
{
    public class CreateGenreRequestDTO
    {
        public IOutputCacheStore OutputCacheStore { get; set; } = null!;
        public IGenreRepository GenresRepository { get; set; } = null!;
        public IMapper Mapper { get; set; } = null!;
    }
}
