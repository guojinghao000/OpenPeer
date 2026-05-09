using Mapster;
using OpenPeer.Application.DTOs.Auth;
using OpenPeer.Domain.Entities;

namespace OpenPeer.Application.Mappings;

public static class MapsterConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<User, UserDto>.NewConfig()
            .Map(dest => dest.Role, src => src.Role.ToString());
    }
}
