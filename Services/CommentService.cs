using BlogApi.Constants;
using BlogApi.Data;
using BlogApi.Dtos.Comments;
using BlogApi.Exceptions;
using BlogApi.Interfaces;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Services;

public class CommentService : ICommentService
{
    private readonly AppDbContext _context;

    public CommentService(
        AppDbContext context
    )
    {
        _context = context;
    }

    public async Task<List<CommentDto>> GetByPostIdAsync(
    int postId
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

        var comments =
            await _context.Comments
                .AsNoTracking()
                .Include(x => x.User)
                .Where(
                    x => x.PostId == postId
                )
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

        var commentMap =
            comments.ToDictionary(
                comment => comment.Id,
                comment => new CommentDto
                {
                    Id = comment.Id,

                    Content = comment.Content,

                    CreatedAt =
                        comment.CreatedAt,

                    UpdatedAt =
                        comment.UpdatedAt,

                    UserId =
                        comment.UserId,

                    UserName =
                        comment.User.Name,

                    PostId =
                        comment.PostId,

                    ParentCommentId =
                        comment.ParentCommentId,

                    Replies =
                        new List<CommentDto>()
                }
            );

        var rootComments =
            new List<CommentDto>();

        foreach (var comment in comments)
        {
            var dto =
                commentMap[comment.Id];

            if (
                comment.ParentCommentId.HasValue
                &&
                commentMap.TryGetValue(
                    comment.ParentCommentId.Value,
                    out var parentDto
                )
            )
            {
                parentDto.Replies.Add(dto);
            }
            else
            {
                rootComments.Add(dto);
            }
        }

        return rootComments
            .OrderByDescending(
                x => x.CreatedAt
            )
            .ToList();
    }

    public async Task<CommentDto> CreateAsync(
        int postId,
        CreateCommentDto dto,
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

        var user =
            await _context.Users
                .FirstOrDefaultAsync(
                    x => x.Id == userId
                );

        if (user == null)
        {
            throw new AppException(
                "Kullanıcı bulunamadı.",
                404,
                "userNotFound"
            );
        }

        var content = dto.Content.Trim();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new AppException(
                "Yorum boş olamaz.",
                400,
                "commentIsRequired"
            );
        }

        var comment = new Comment
        {
            Content = content,
            UserId = userId,
            PostId = postId
        };

        _context.Comments.Add(comment);

        await _context.SaveChangesAsync();

        return new CommentDto
        {
            Id = comment.Id,

            Content = comment.Content,

            CreatedAt = comment.CreatedAt,

            UpdatedAt = comment.UpdatedAt,

            UserId = comment.UserId,

            UserName = user.Name,

            PostId = comment.PostId,

            ParentCommentId = null
        };
    }

    public async Task<CommentDto> UpdateAsync(
        int commentId,
        UpdateCommentDto dto,
        int userId,
        string role
    )
    {
        var comment =
            await _context.Comments
                .Include(x => x.User)
                .FirstOrDefaultAsync(
                    x => x.Id == commentId
                );

        if (comment == null)
        {
            throw new AppException(
                "Yorum bulunamadı.",
                404,
                "commentNotFound"
            );
        }

        var isAdmin =
            role == Roles.Admin ||
            role == Roles.SuperAdmin;

        if (
            !isAdmin &&
            comment.UserId != userId
        )
        {
            throw new AppException(
                "Bu yorumu güncelleme yetkiniz yok.",
                403,
                "commentUpdateForbidden"
            );
        }

        var content = dto.Content.Trim();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new AppException(
                "Yorum boş olamaz.",
                400,
                "commentIsRequired"
            );
        }

        comment.Content = content;

        comment.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new CommentDto
        {
            Id = comment.Id,

            Content = comment.Content,

            CreatedAt = comment.CreatedAt,

            UpdatedAt = comment.UpdatedAt,

            UserId = comment.UserId,

            UserName = comment.User.Name,

            PostId = comment.PostId
        };
    }

    public async Task DeleteAsync(
        int commentId,
        int userId,
        string role
    )
    {
        var comment =
            await _context.Comments
                .FirstOrDefaultAsync(
                    x => x.Id == commentId
                );

        if (comment == null)
        {
            throw new AppException(
                "Yorum bulunamadı.",
                404,
                "commentNotFound"
            );
        }

        var isAdmin =
            role == Roles.Admin ||
            role == Roles.SuperAdmin;

        if (
            !isAdmin &&
            comment.UserId != userId
        )
        {
            throw new AppException(
                "Bu yorumu silme yetkiniz yok.",
                403,
                "commentDeleteForbidden"
            );
        }

        _context.Comments.Remove(comment);

        await _context.SaveChangesAsync();
    }


    public async Task<CommentDto> CreateReplyAsync(
        int parentCommentId,
        CreateReplyDto dto,
        int userId
    )
    {
        var parentComment =
            await _context.Comments
                .FirstOrDefaultAsync(
                    x => x.Id == parentCommentId
                );

        if (parentComment == null)
        {
            throw new AppException(
                "Yanıt verilecek yorum bulunamadı.",
                404,
                "parentCommentNotFound"
            );
        }

        var user =
            await _context.Users
                .FirstOrDefaultAsync(
                    x => x.Id == userId
                );

        if (user == null)
        {
            throw new AppException(
                "Kullanıcı bulunamadı.",
                404,
                "userNotFound"
            );
        }

        var content = dto.Content.Trim();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new AppException(
                "Yorum boş olamaz.",
                400,
                "commentIsRequired"
            );
        }

        var reply = new Comment
        {
            Content = content,

            UserId = userId,

            PostId = parentComment.PostId,

            ParentCommentId = parentComment.Id
        };

        _context.Comments.Add(reply);

        await _context.SaveChangesAsync();

        return new CommentDto
        {
            Id = reply.Id,

            Content = reply.Content,

            CreatedAt = reply.CreatedAt,

            UpdatedAt = reply.UpdatedAt,

            UserId = reply.UserId,

            UserName = user.Name,

            PostId = reply.PostId,

            ParentCommentId =
                reply.ParentCommentId,

            Replies = new List<CommentDto>()
        };
    }
}