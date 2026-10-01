using BlogApi.Data;
using BlogApi.Dtos.Categories;
using BlogApi.Exceptions;
using BlogApi.Interfaces;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        return await _context.Categories
            .OrderBy(x => x.Name)
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<CategoryDto> GetByIdAsync(int id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == id);

        if (category == null)
        {
            throw new AppException(
                "Kategori bulunamadı.",
                404
            );
        }

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            CreatedAt = category.CreatedAt
        };
    }

    public async Task<CategoryDto> CreateAsync(
        CreateCategoryDto dto
    )
    {
        var exists = await _context.Categories
            .AnyAsync(x => x.Name == dto.Name);

        if (exists)
        {
            throw new AppException(
                "Bu kategori zaten mevcut.",
                400
            );
        }

        var category = new Category
        {
            Name = dto.Name
        };

        _context.Categories.Add(category);

        await _context.SaveChangesAsync();

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            CreatedAt = category.CreatedAt
        };
    }

    public async Task<CategoryDto> UpdateAsync(
        int id,
        UpdateCategoryDto dto
    )
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == id);

        if (category == null)
        {
            throw new AppException(
                "Kategori bulunamadı.",
                404
            );
        }

        category.Name = dto.Name;

        await _context.SaveChangesAsync();

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            CreatedAt = category.CreatedAt
        };
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == id);

        if (category == null)
        {
            throw new AppException(
                "Kategori bulunamadı.",
                404
            );
        }

        var hasPosts = await _context.Posts
            .AnyAsync(x => x.Categories.Any(c => c.Id == id));

        if (hasPosts)
        {
            throw new AppException(
                "Bu kategoriye bağlı postlar olduğu için silinemez.",
                400
            );
        }

        _context.Categories.Remove(category);

        await _context.SaveChangesAsync();
    }
}