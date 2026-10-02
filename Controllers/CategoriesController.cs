using BlogApi.Dtos.Categories;
using BlogApi.Interfaces;
using BlogApi.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApi.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(
        ICategoryService categoryService
    )
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories =
            await _categoryService.GetAllAsync();

        return Ok(
            ApiResponse<List<CategoryDto>>.Success(
                categories,
                200,
                "Kategoriler getirildi."
            )
        );
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category =
            await _categoryService.GetByIdAsync(id);

        return Ok(
            ApiResponse<CategoryDto>.Success(
                category,
                200,
                "Kategori getirildi."
            )
        );
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateCategoryDto dto
    )
    {
        var category =
            await _categoryService.CreateAsync(dto);

        return StatusCode(
            201,
            ApiResponse<CategoryDto>.Success(
                category,
                201,
                "Kategori oluşturuldu."
            )
        );
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateCategoryDto dto
    )
    {
        var category =
            await _categoryService.UpdateAsync(id, dto);

        return Ok(
            ApiResponse<CategoryDto>.Success(
                category,
                200,
                "Kategori güncellendi."
            )
        );
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _categoryService.DeleteAsync(id);

        return Ok(
            ApiResponse<object>.Success(
                new { },
                200,
                "Kategori silindi."
            )
        );
    }
}