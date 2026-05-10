using Microsoft.AspNetCore.Mvc;
using OpenPeer.Application.DTOs.Categories;
using OpenPeer.Application.DTOs.Common;
using OpenPeer.Application.Interfaces;

namespace OpenPeer.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoriesController(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        var categories = await _categoryRepository.GetAllAsync();
        var dtos = categories.Select(c => new CategoryListDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            PaperCount = c.PaperCategories.Count,
            CreatedAt = c.CreatedAt
        }).ToList();

        return Ok(ApiResponse<List<CategoryListDto>>.Success(dtos));
    }
}
