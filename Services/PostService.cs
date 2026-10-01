using BlogApi.Data;
using BlogApi.Dtos.Categories;
using BlogApi.Dtos.Posts;
using BlogApi.Exceptions;
using BlogApi.Interfaces;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Services;

public class PostService : IPostService
{
    private readonly AppDbContext _context;

    public PostService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PostDto>> GetAllAsync()
    {
        return await _context.Posts
        .Include(x => x.User)
        .Include(x => x.Categories)
        .OrderByDescending(x => x.CreatedAt)
        .Select(x => new PostDto
        {
            Id = x.Id,
            Title = x.Title,
            Content = x.Content,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            UserId = x.UserId,
            AuthorName = x.User.Name,
            Categories = x.Categories
                .Select(category => new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    CreatedAt = category.CreatedAt
                })
                .ToList()
        })
        .ToListAsync();
    }

    public async Task<PostDto> GetByIdAsync(int id)
    {
        var post = await _context.Posts
            .Include(x => x.User)
            .Include(x => x.Categories)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (post == null)
        {
            throw new AppException(
                "Post bulunamadı.",
                404
            );
        }

        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            UserId = post.UserId,
            AuthorName = post.User.Name,
            Categories = post.Categories
        .Select(x => new CategoryDto
        {
            Id = x.Id,
            Name = x.Name,
            CreatedAt = x.CreatedAt
        })
        .ToList()
        };
    }

    public async Task<PostDto> CreateAsync(
    CreatePostDto dto,
    int userId
)
    {
        var categories = await _context.Categories
            .Where(x => dto.CategoryIds.Contains(x.Id))
            .ToListAsync();

        if (categories.Count != dto.CategoryIds.Distinct().Count())
        {
            throw new AppException(
                "Gönderilen kategorilerden biri veya birkaçı bulunamadı.",
                404
            );
        }

        var post = new Post
        {
            Title = dto.Title,
            Content = dto.Content,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            Categories = categories
        };

        _context.Posts.Add(post);

        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);

        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            UserId = post.UserId,
            AuthorName = user?.Name ?? string.Empty,
            Categories = categories
                .Select(x => new CategoryDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    CreatedAt = x.CreatedAt
                })
                .ToList()
        };
    }

    public async Task<PostDto> UpdateAsync(
      int id,
      UpdatePostDto dto,
      int userId
  )
    {
        var post = await _context.Posts
            .Include(x => x.User)
            .Include(x => x.Categories)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (post == null)
        {
            throw new AppException(
                "Post bulunamadı.",
                404
            );
        }

        if (post.UserId != userId)
        {
            throw new AppException(
                "Bu postu güncelleme yetkiniz yok.",
                403
            );
        }

        var categories = await _context.Categories
            .Where(x => dto.CategoryIds.Contains(x.Id))
            .ToListAsync();

        if (categories.Count != dto.CategoryIds.Distinct().Count())
        {
            throw new AppException(
                "Gönderilen kategorilerden biri veya birkaçı bulunamadı.",
                404
            );
        }

        post.Title = dto.Title;
        post.Content = dto.Content;
        post.UpdatedAt = DateTime.UtcNow;

        post.Categories.Clear();

        foreach (var category in categories)
        {
            post.Categories.Add(category);
        }

        await _context.SaveChangesAsync();

        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            UserId = post.UserId,
            AuthorName = post.User.Name,
            Categories = post.Categories
                .Select(x => new CategoryDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    CreatedAt = x.CreatedAt
                })
                .ToList()
        };
    }
    public async Task DeleteAsync(
        int id,
        int userId
    )
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(x => x.Id == id);

        if (post == null)
        {
            throw new AppException(
                "Post bulunamadı.",
                404
            );
        }

        if (post.UserId != userId)
        {
            throw new AppException(
                "Bu postu silme yetkiniz yok.",
                403
            );
        }

        _context.Posts.Remove(post);

        await _context.SaveChangesAsync();
    }
}