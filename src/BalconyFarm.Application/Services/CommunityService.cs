using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;
using BalconyFarm.Domain.Entities;
using BalconyFarm.Domain.Interfaces;
using Mapster;
using Microsoft.Extensions.Logging;

namespace BalconyFarm.Application.Services;

public class CommunityService : ICommunityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CommunityService> _logger;
    private readonly IAchievementService _achievementService;

    public CommunityService(IUnitOfWork unitOfWork, ILogger<CommunityService> logger, IAchievementService achievementService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _achievementService = achievementService;
    }

    public async Task<ApiResponse<PagedResult<CommunityQuestionDto>>> GetQuestionsAsync(QuestionQueryRequestDto query, CancellationToken cancellationToken = default)
    {
        var questionsList = (await _unitOfWork.Questions.GetAllAsync(cancellationToken)).ToList();

        IEnumerable<CommunityQuestion> questions = questionsList;

        if (!string.IsNullOrEmpty(query.CropType))
        {
            questions = questions.Where(q => q.CropType == query.CropType);
        }

        if (!string.IsNullOrEmpty(query.Tag))
        {
            questions = questions.Where(q => q.Tags.Any(t => t.Name == query.Tag));
        }

        if (query.IsResolved.HasValue)
        {
            questions = questions.Where(q => q.IsResolved == query.IsResolved.Value);
        }

        if (!string.IsNullOrEmpty(query.SearchKeyword))
        {
            questions = questions.Where(q =>
                q.Title.Contains(query.SearchKeyword) ||
                q.Content.Contains(query.SearchKeyword));
        }

        var totalCount = questions.Count();

        questions = (query.SortBy?.ToLower()) switch
        {
            "viewcount" => query.SortOrder?.ToLower() == "asc"
                ? questions.OrderBy(q => q.ViewCount)
                : questions.OrderByDescending(q => q.ViewCount),
            "replycount" => query.SortOrder?.ToLower() == "asc"
                ? questions.OrderBy(q => q.Replies.Count)
                : questions.OrderByDescending(q => q.Replies.Count),
            _ => query.SortOrder?.ToLower() == "asc"
                ? questions.OrderBy(q => q.CreatedAt)
                : questions.OrderByDescending(q => q.CreatedAt)
        };

        var items = questions
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(q => MapQuestionToDto(q))
            .ToList();

        var result = new PagedResult<CommunityQuestionDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };

        return ApiResponse<PagedResult<CommunityQuestionDto>>.Success(result);
    }

    public async Task<ApiResponse<QuestionDetailDto>> GetQuestionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var question = await _unitOfWork.Questions.GetByIdAsync(id, cancellationToken);
        if (question == null)
        {
            return ApiResponse<QuestionDetailDto>.Error("问题不存在", 404);
        }

        question.ViewCount++;
        await _unitOfWork.Questions.UpdateAsync(question, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var detail = new QuestionDetailDto
        {
            Question = MapQuestionToDto(question),
            Replies = question.Replies.Select(r => new CommunityReplyDto
            {
                Id = r.Id,
                QuestionId = r.QuestionId,
                UserId = r.UserId,
                Content = r.Content,
                IsAccepted = r.IsAccepted,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                AuthorUsername = r.User?.Username
            }).ToList()
        };

        return ApiResponse<QuestionDetailDto>.Success(detail);
    }

    public async Task<ApiResponse<CommunityQuestionDto>> CreateQuestionAsync(CreateQuestionRequestDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("创建问题: {Title}, 用户: {UserId}", dto.Title, userId);

        var question = new CommunityQuestion
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = dto.Title,
            Content = dto.Content,
            CropType = dto.CropType,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (dto.Tags != null && dto.Tags.Any())
        {
            foreach (var tagName in dto.Tags.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                question.Tags.Add(new CommunityTag
                {
                    Id = Guid.NewGuid(),
                    QuestionId = question.Id,
                    Name = tagName,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _unitOfWork.Questions.AddAsync(question, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("问题创建成功: {QuestionId}", question.Id);

        var questionDto = MapQuestionToDto(question);
        return ApiResponse<CommunityQuestionDto>.Success(questionDto, "创建成功");
    }

    public async Task<ApiResponse<CommunityQuestionDto>> UpdateQuestionAsync(Guid id, UpdateQuestionRequestDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("更新问题: {QuestionId}, 用户: {UserId}", id, userId);

        var question = await _unitOfWork.Questions.GetByIdAsync(id, cancellationToken);
        if (question == null)
        {
            return ApiResponse<CommunityQuestionDto>.Error("问题不存在", 404);
        }

        if (question.UserId != userId)
        {
            return ApiResponse<CommunityQuestionDto>.Error("无权修改此问题", 403);
        }

        if (!string.IsNullOrEmpty(dto.Title))
            question.Title = dto.Title;
        if (!string.IsNullOrEmpty(dto.Content))
            question.Content = dto.Content;
        if (dto.CropType != null)
            question.CropType = dto.CropType;
        if (dto.IsResolved.HasValue)
            question.IsResolved = dto.IsResolved.Value;

        if (dto.Tags != null)
        {
            var existingTags = question.Tags.ToList();
            foreach (var tag in existingTags)
            {
                await _unitOfWork.Tags.DeleteAsync(tag, cancellationToken);
            }
            question.Tags.Clear();

            foreach (var tagName in dto.Tags.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var newTag = new CommunityTag
                {
                    Id = Guid.NewGuid(),
                    QuestionId = question.Id,
                    Name = tagName,
                    CreatedAt = DateTime.UtcNow
                };
                question.Tags.Add(newTag);
            }
        }

        question.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Questions.UpdateAsync(question, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("问题更新成功: {QuestionId}", id);

        var questionDto = MapQuestionToDto(question);
        return ApiResponse<CommunityQuestionDto>.Success(questionDto, "更新成功");
    }

    public async Task<ApiResponse> DeleteQuestionAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("删除问题: {QuestionId}, 用户: {UserId}", id, userId);

        var question = await _unitOfWork.Questions.GetByIdAsync(id, cancellationToken);
        if (question == null)
        {
            return ApiResponse.Error("问题不存在", 404);
        }

        if (question.UserId != userId)
        {
            return ApiResponse.Error("无权删除此问题", 403);
        }

        await _unitOfWork.Questions.DeleteAsync(question, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("问题删除成功: {QuestionId}", id);

        return ApiResponse.Success(null, "删除成功");
    }

    public async Task<ApiResponse<CommunityReplyDto>> CreateReplyAsync(Guid questionId, CreateReplyRequestDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("创建回复: 问题 {QuestionId}, 用户: {UserId}", questionId, userId);

        var question = await _unitOfWork.Questions.GetByIdAsync(questionId, cancellationToken);
        if (question == null)
        {
            return ApiResponse<CommunityReplyDto>.Error("问题不存在", 404);
        }

        var reply = new CommunityReply
        {
            Id = Guid.NewGuid(),
            QuestionId = questionId,
            UserId = userId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Replies.AddAsync(reply, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("回复创建成功: {ReplyId}", reply.Id);

        var replyDto = new CommunityReplyDto
        {
            Id = reply.Id,
            QuestionId = reply.QuestionId,
            UserId = reply.UserId,
            Content = reply.Content,
            IsAccepted = reply.IsAccepted,
            CreatedAt = reply.CreatedAt,
            UpdatedAt = reply.UpdatedAt,
            AuthorUsername = reply.User?.Username
        };

        return ApiResponse<CommunityReplyDto>.Success(replyDto, "回复成功");
    }

    public async Task<ApiResponse<CommunityReplyDto>> UpdateReplyAsync(Guid replyId, UpdateReplyRequestDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("更新回复: {ReplyId}, 用户: {UserId}", replyId, userId);

        var reply = await _unitOfWork.Replies.GetByIdAsync(replyId, cancellationToken);
        if (reply == null)
        {
            return ApiResponse<CommunityReplyDto>.Error("回复不存在", 404);
        }

        if (reply.UserId != userId)
        {
            return ApiResponse<CommunityReplyDto>.Error("无权修改此回复", 403);
        }

        if (!string.IsNullOrEmpty(dto.Content))
            reply.Content = dto.Content;

        reply.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Replies.UpdateAsync(reply, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("回复更新成功: {ReplyId}", replyId);

        var replyDto = new CommunityReplyDto
        {
            Id = reply.Id,
            QuestionId = reply.QuestionId,
            UserId = reply.UserId,
            Content = reply.Content,
            IsAccepted = reply.IsAccepted,
            CreatedAt = reply.CreatedAt,
            UpdatedAt = reply.UpdatedAt,
            AuthorUsername = reply.User?.Username
        };

        return ApiResponse<CommunityReplyDto>.Success(replyDto, "更新成功");
    }

    public async Task<ApiResponse> DeleteReplyAsync(Guid replyId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("删除回复: {ReplyId}, 用户: {UserId}", replyId, userId);

        var reply = await _unitOfWork.Replies.GetByIdAsync(replyId, cancellationToken);
        if (reply == null)
        {
            return ApiResponse.Error("回复不存在", 404);
        }

        if (reply.UserId != userId)
        {
            return ApiResponse.Error("无权删除此回复", 403);
        }

        await _unitOfWork.Replies.DeleteAsync(reply, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("回复删除成功: {ReplyId}", replyId);

        return ApiResponse.Success(null, "删除成功");
    }

    public async Task<ApiResponse<CommunityReplyDto>> AcceptReplyAsync(Guid questionId, Guid replyId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("采纳回复: 问题 {QuestionId}, 回复 {ReplyId}, 用户: {UserId}", questionId, replyId, userId);

        var question = await _unitOfWork.Questions.GetByIdAsync(questionId, cancellationToken);
        if (question == null)
        {
            return ApiResponse<CommunityReplyDto>.Error("问题不存在", 404);
        }

        if (question.UserId != userId)
        {
            return ApiResponse<CommunityReplyDto>.Error("只有提问者才能采纳回复", 403);
        }

        var reply = await _unitOfWork.Replies.GetByIdAsync(replyId, cancellationToken);
        if (reply == null || reply.QuestionId != questionId)
        {
            return ApiResponse<CommunityReplyDto>.Error("回复不存在或不属于该问题", 404);
        }

        foreach (var existingReply in question.Replies.Where(r => r.IsAccepted))
        {
            existingReply.IsAccepted = false;
            await _unitOfWork.Replies.UpdateAsync(existingReply, cancellationToken);
        }

        reply.IsAccepted = true;
        question.IsResolved = true;
        question.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Replies.UpdateAsync(reply, cancellationToken);
        await _unitOfWork.Questions.UpdateAsync(question, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("回复采纳成功: {ReplyId}", replyId);

        await _achievementService.CheckAndUnlockCommunityAchievementsAsync(reply.UserId, cancellationToken);

        var replyDto = new CommunityReplyDto
        {
            Id = reply.Id,
            QuestionId = reply.QuestionId,
            UserId = reply.UserId,
            Content = reply.Content,
            IsAccepted = reply.IsAccepted,
            CreatedAt = reply.CreatedAt,
            UpdatedAt = reply.UpdatedAt,
            AuthorUsername = reply.User?.Username
        };

        return ApiResponse<CommunityReplyDto>.Success(replyDto, "采纳成功");
    }

    public async Task<ApiResponse<List<HotTopicDto>>> GetHotTopicsAsync(CancellationToken cancellationToken = default)
    {
        var questions = await _unitOfWork.Questions.GetAllAsync(cancellationToken);

        var hotTopics = questions
            .Where(q => !string.IsNullOrEmpty(q.CropType))
            .GroupBy(q => q.CropType!)
            .Select(g => new HotTopicDto
            {
                CropType = g.Key,
                QuestionCount = g.Count()
            })
            .OrderByDescending(t => t.QuestionCount)
            .Take(20)
            .ToList();

        return ApiResponse<List<HotTopicDto>>.Success(hotTopics);
    }

    public async Task<ApiResponse<List<HotTagDto>>> GetHotTagsAsync(CancellationToken cancellationToken = default)
    {
        var questions = await _unitOfWork.Questions.GetAllAsync(cancellationToken);

        var hotTags = questions
            .SelectMany(q => q.Tags)
            .GroupBy(t => t.Name)
            .Select(g => new HotTagDto
            {
                Name = g.Key,
                QuestionCount = g.Count()
            })
            .OrderByDescending(t => t.QuestionCount)
            .Take(20)
            .ToList();

        return ApiResponse<List<HotTagDto>>.Success(hotTags);
    }

    private static CommunityQuestionDto MapQuestionToDto(CommunityQuestion q)
    {
        return new CommunityQuestionDto
        {
            Id = q.Id,
            UserId = q.UserId,
            Title = q.Title,
            Content = q.Content,
            CropType = q.CropType,
            ViewCount = q.ViewCount,
            IsResolved = q.IsResolved,
            CreatedAt = q.CreatedAt,
            UpdatedAt = q.UpdatedAt,
            AuthorUsername = q.User?.Username,
            ReplyCount = q.Replies.Count,
            Tags = q.Tags.Select(t => t.Name).ToList()
        };
    }
}
