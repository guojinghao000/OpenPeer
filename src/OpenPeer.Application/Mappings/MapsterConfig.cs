using Mapster;
using OpenPeer.Application.DTOs.Auth;
using OpenPeer.Application.DTOs.Papers;
using OpenPeer.Domain.Entities;

namespace OpenPeer.Application.Mappings;

public static class MapsterConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<User, UserDto>.NewConfig()
            .Map(dest => dest.Role, src => src.Role.ToString());

        TypeAdapterConfig<Paper, PaperDto>.NewConfig()
            .Map(dest => dest.Author, src => src.Author)
            .Map(dest => dest.Categories,
                src => src.PaperCategories.Select(pc => new CategoryDto
                {
                    Id = pc.CategoryId,
                    Name = pc.Category.Name
                }).ToList())
            .Map(dest => dest.Status, src => src.Status.ToString());

        TypeAdapterConfig<Paper, PaperDetailDto>.NewConfig()
            .Map(dest => dest.Author, src => src.Author)
            .Map(dest => dest.Categories,
                src => src.PaperCategories.Select(pc => new CategoryDto
                {
                    Id = pc.CategoryId,
                    Name = pc.Category.Name
                }).ToList())
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Ignore(dest => dest.FileUrl)
            .Ignore(dest => dest.RatingDistribution)
            .Ignore(dest => dest.CurrentUserRating);
    }
}
