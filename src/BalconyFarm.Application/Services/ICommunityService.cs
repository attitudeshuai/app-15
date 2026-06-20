using BalconyFarm.Application.DTOs;
using BalconyFarm.Application.Models;

namespace BalconyFarm.Application.Services;

public interface ICommunityService
{
    Task<ApiResponse<PagedResult<CommunityQuestionDto>>> GetQuestionsAsync(QuestionQueryRequestDto query, CancellationToken cancellationToken = default);
    Task<ApiResponse<QuestionDetailDto>> GetQuestionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CommunityQuestionDto>> CreateQuestionAsync(CreateQuestionRequestDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CommunityQuestionDto>> UpdateQuestionAsync(Guid id, UpdateQuestionRequestDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteQuestionAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CommunityReplyDto>> CreateReplyAsync(Guid questionId, CreateReplyRequestDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CommunityReplyDto>> UpdateReplyAsync(Guid replyId, UpdateReplyRequestDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse> DeleteReplyAsync(Guid replyId, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CommunityReplyDto>> AcceptReplyAsync(Guid questionId, Guid replyId, Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<HotTopicDto>>> GetHotTopicsAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<List<HotTagDto>>> GetHotTagsAsync(CancellationToken cancellationToken = default);
}
