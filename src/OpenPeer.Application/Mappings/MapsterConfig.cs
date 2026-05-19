using Mapster;
using OpenPeer.Application.DTOs.Auth;
using OpenPeer.Application.DTOs.Comments;
using OpenPeer.Application.DTOs.Papers;
using OpenPeer.Application.DTOs.Ratings;
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

        TypeAdapterConfig<Rating, RatingDto>.NewConfig()
            .Map(dest => dest.User, src => src.User);

        TypeAdapterConfig<Comment, CommentDto>.NewConfig()
            .Map(dest => dest.User, src => src.User);
    }
}
