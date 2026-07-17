using AutoMapper;
using MinimalApiMovies.DTOs;
using MinimalApiMovies.Entities;

namespace MinimalApiMovies.Mappers
{
    public class ActorProfile : Profile
    {
        public ActorProfile()
        {
            CreateMap<CreateActorDTO, Actor>();
            CreateMap<Actor, ActorDTO>();
        }
    }
}
