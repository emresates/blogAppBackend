using BlogApi.Constants;
using BlogApi.Data;
using BlogApi.Dtos.Categories;
using BlogApi.Dtos.Posts;
using BlogApi.Exceptions;
using BlogApi.Helpers;
using BlogApi.Interfaces;
using BlogApi.Models;
using BlogApi.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Services;

public class PostService : IPostService
{
    private readonly AppDbContext _context;

    public PostService(AppDbContext context)
    {
        _context = context;
    }

    private async Task<string> GenerateUniqueSlugAsync(
    string title,
    int? excludePostId = null
)
    {
        var baseSlug = SlugHelper.Generate(title);

        if (string.IsNullOrWhiteSpace(baseSlug))
        {
            baseSlug = "post";
        }

        var slug = baseSlug;

        var counter = 2;

        while (
            await _context.Posts.AnyAsync(
                x =>
                    x.Slug == slug &&
                    (
                        !excludePostId.HasValue ||
                        x.Id != excludePostId.Value
                    )
            )
        )
        {
            slug = $"{baseSlug}-{counter}";

            counter++;
        }

        return slug;
    }

    public async Task<PagedResult<PostDto>> GetAllAsync(
        int? categoryId,
        string? search,
        int page,
        int pageSize
    )
    {
        var query = _context.Posts
            .AsNoTracking()
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(
                post =>
                    post.Categories.Any(
                        category =>
                            category.Id ==
                            categoryId.Value
                    )
            );
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchText =
                $"%{search.Trim()}%";

            query = query.Where(
                post =>
                    EF.Functions.ILike(
                        post.Title,
                        searchText
                    )
                    ||
                    EF.Functions.ILike(
                        post.Content,
                        searchText
                    )
            );
        }

        var totalCount =
            await query.CountAsync();


        var posts = await query
            .OrderByDescending(
                post => post.CreatedAt
            )
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(post => new PostDto
            {
                Id = post.Id,

                Title = post.Title,

                Slug = post.Slug,

                Content = post.Content,

                ViewCount = post.ViewCount,

                LikeCount = post.Likes.Count,

                CreatedAt = post.CreatedAt,

                UpdatedAt = post.UpdatedAt,

                UserId = post.UserId,

                AuthorName = post.User.Name,

                Categories = post.Categories
                    .Select(category =>
                        new CategoryDto
                        {
                            Id = category.Id,
                            Name = category.Name
                        }
                    )
                    .ToList(),

                CommentCount = post.Comments.Count,

            })
            .ToListAsync();

        var totalPages =
            (int)Math.Ceiling(
                totalCount /
                (double)pageSize
            );

        return new PagedResult<PostDto>
        {
            Items = posts,

            Pagination = new PaginationMeta
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            }
        };
    }

    public async Task<PostDto> GetByIdAsync(
        int id
    )
    {
        var post = await _context.Posts
            .Include(x => x.User)
            .Include(x => x.Categories)
            .Include(x => x.Likes)
            .Include(x => x.Comments)
            .FirstOrDefaultAsync(
                x => x.Id == id
            );

        if (post == null)
        {
            throw new AppException(
                "Post bulunamadı.",
                404,
                "postNotFound"
            );
        }

        post.ViewCount++;

        await _context.SaveChangesAsync();

        return MapToDto(post);
    }

    public async Task<PostDto> GetBySlugAsync(
        string slug
    )
    {
        var post = await _context.Posts
            .Include(x => x.User)
            .Include(x => x.Categories)
            .Include(x => x.Likes)
            .Include(x => x.Comments)
            .FirstOrDefaultAsync(
                x => x.Slug == slug
            );

        if (post == null)
        {
            throw new AppException(
                "Post bulunamadı.",
                404,
                "postNotFound"
            );
        }

        post.ViewCount++;

        await _context.SaveChangesAsync();

        return MapToDto(post);
    }

    public async Task<PostDto> CreateAsync(
        CreatePostDto dto,
        int userId
    )
    {
        var slug =
            await GenerateUniqueSlugAsync(
                dto.Title
            );
        var userExists =
            await _context.Users
                .AnyAsync(
                    x => x.Id == userId
                );

        if (!userExists)
        {
            throw new AppException(
                "Kullanıcı bulunamadı.",
                404,
                "userNotFound"
            );
        }

        var categoryIds =
            dto.CategoryIds
                .Distinct()
                .ToList();

        var categories =
            await _context.Categories
                .Where(
                    x =>
                        categoryIds.Contains(
                            x.Id
                        )
                )
                .ToListAsync();

        if (
            categories.Count !=
            categoryIds.Count
        )
        {
            throw new AppException(
                "Kategori bulunamadı.",
                404,
                "categoryNotFound"
            );
        }

        var post = new Post
        {
            Slug = slug,

            Title = dto.Title.Trim(),

            Content = dto.Content.Trim(),

            UserId = userId,

            Categories = categories,

            ViewCount = 0
        };

        _context.Posts.Add(post);

        await _context.SaveChangesAsync();

        post = await _context.Posts
            .Include(x => x.User)
            .Include(x => x.Categories)
            .Include(x => x.Likes)
            .Include(x => x.Comments)
            .FirstAsync(
                x => x.Id == post.Id
            );

        return MapToDto(post);
    }

    public async Task<PostDto> UpdateAsync(
        int id,
        UpdatePostDto dto,
        int userId,
        string role
    )
    {
        var post = await _context.Posts
            .Include(x => x.User)
            .Include(x => x.Categories)
            .Include(x => x.Likes)
            .Include(x => x.Comments)
            .FirstOrDefaultAsync(
                x => x.Id == id
            );


        if (post == null)
        {
            throw new AppException(
                "Post bulunamadı.",
                404,
                "postNotFound"
            );
        }

        var isAdmin =
            role == Roles.Admin ||
            role == Roles.SuperAdmin;

        if (
            !isAdmin &&
            post.UserId != userId
        )
        {
            throw new AppException(
                "Bu postu güncelleme yetkiniz yok.",
                403,
                "postUpdateForbidden"
            );
        }

        var categoryIds =
            dto.CategoryIds
                .Distinct()
                .ToList();

        var categories =
            await _context.Categories
                .Where(
                    x =>
                        categoryIds.Contains(
                            x.Id
                        )
                )
                .ToListAsync();

        if (
            categories.Count !=
            categoryIds.Count
        )
        {
            throw new AppException(
                "Kategori bulunamadı.",
                404,
                "categoryNotFound"
            );
        }

        var slug =
            await GenerateUniqueSlugAsync(
                dto.Title,
                post.Id
        );


        post.Title = dto.Title.Trim();

        post.Content =
            dto.Content.Trim();

        post.UpdatedAt =
            DateTime.UtcNow;

        post.Slug = slug;

        post.Categories.Clear();

        foreach (var category in categories)
        {
            post.Categories.Add(category);
        }

        await _context.SaveChangesAsync();

        return MapToDto(post);
    }

    public async Task DeleteAsync(
        int id,
        int userId,
        string role
    )
    {
        var post =
            await _context.Posts
                .FirstOrDefaultAsync(
                    x => x.Id == id
                );

        if (post == null)
        {
            throw new AppException(
                "Post bulunamadı.",
                404,
                "postNotFound"
            );
        }

        var isAdmin =
            role == Roles.Admin ||
            role == Roles.SuperAdmin;

        if (
            !isAdmin &&
            post.UserId != userId
        )
        {
            throw new AppException(
                "Bu postu silme yetkiniz yok.",
                403,
                "postDeleteForbidden"
            );
        }

        _context.Posts.Remove(post);

        await _context.SaveChangesAsync();
    }

    public async Task LikeAsync(
        int postId,
        int userId
    )
    {
        var postExists =
            await _context.Posts
                .AnyAsync(
                    x => x.Id == postId
                );

        if (!postExists)
        {
            throw new AppException(
                "Post bulunamadı.",
                404,
                "postNotFound"
            );
        }

        var alreadyLiked =
            await _context.PostLikes
                .AnyAsync(
                    x =>
                        x.PostId == postId &&
                        x.UserId == userId
                );

        if (alreadyLiked)
        {
            throw new AppException(
                "Bu post zaten beğenilmiş.",
                400,
                "postAlreadyLiked"
            );
        }

        var like = new PostLike
        {
            PostId = postId,
            UserId = userId
        };

        _context.PostLikes.Add(like);

        await _context.SaveChangesAsync();
    }

    public async Task UnlikeAsync(
        int postId,
        int userId
    )
    {
        var like =
            await _context.PostLikes
                .FirstOrDefaultAsync(
                    x =>
                        x.PostId == postId &&
                        x.UserId == userId
                );

        if (like == null)
        {
            throw new AppException(
                "Bu post beğenilmemiş.",
                400,
                "postNotLiked"
            );
        }

        _context.PostLikes.Remove(like);

        await _context.SaveChangesAsync();
    }

    private static PostDto MapToDto(
        Post post
    )
    {
        return new PostDto
        {
            Id = post.Id,

            Title = post.Title,

            Slug = post.Slug,

            Content = post.Content,

            ViewCount = post.ViewCount,

            LikeCount = post.Likes.Count,

            CreatedAt = post.CreatedAt,

            UpdatedAt = post.UpdatedAt,

            CommentCount = post.Comments.Count,

            UserId = post.UserId,

            AuthorName = post.User.Name,

            Categories = post.Categories
                .Select(
                    category =>
                        new CategoryDto
                        {
                            Id = category.Id,
                            Name = category.Name
                        }
                )
                .ToList()
        };
    }
}