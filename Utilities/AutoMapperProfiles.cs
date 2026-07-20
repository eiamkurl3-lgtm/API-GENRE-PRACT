using AutoMapper;
using MinimalApiMovies.DTOs;
using MinimalApiMovies.Entities;

namespace MinimalApiMovies.Utilities
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Genre mappings
            CreateMap<CreateGenreDTO, Genre>();
            CreateMap<Genre, GenreDTO>();

            // User mappings
            CreateMap<CreateUserDTO, User>();
            CreateMap<User, UserDTO>();

            // Actor mappings
            CreateMap<CreateActorDTO, Actor>()
                .ForMember(p=> p.ProfilePicture,options=> options.Ignore());
            CreateMap<Actor, ActorDTO>();

            // Movies mappings
            CreateMap<CreateMoviesDTO, Movies>()
                .ForMember(p => p.Poster, options => options.Ignore());
            CreateMap<Movies, MoviesDTO>();

            // Comments mappings
            CreateMap<CreateCommentsDTO, Comments>();
            CreateMap<Comments, CommentsDTO>();
        }
    }
}
