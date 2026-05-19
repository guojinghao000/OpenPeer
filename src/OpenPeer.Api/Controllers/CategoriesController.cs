using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenPeer.Application.DTOs.Categories;
using OpenPeer.Application.DTOs.Common;
using OpenPeer.Application.Interfaces;
using OpenPeer.Domain.Entities;

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

    [HttpPost]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };
        _categoryRepository.Add(category);
        await _categoryRepository.SaveChangesAsync();
        return CreatedAtAction(nameof(GetList), null,
            ApiResponse<object>.Success(201, "分类已创建", new { id = category.Id }));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateCategoryRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category is null)
            return NotFound(ApiResponse.Error(404, "分类不存在"));

        category.Name = request.Name;
        category.Description = request.Description;
        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync();
        return Ok(ApiResponse.Success("分类已更新"));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category is null)
            return NotFound(ApiResponse.Error(404, "分类不存在"));

        await _categoryRepository.DeleteAsync(id);
        await _categoryRepository.SaveChangesAsync();
        return NoContent();
    }
}
