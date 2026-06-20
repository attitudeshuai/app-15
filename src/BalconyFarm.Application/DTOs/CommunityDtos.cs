using BalconyFarm.Application.Models;

namespace BalconyFarm.Application.DTOs;

public class CommunityQuestionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? CropType { get; set; }
    public int ViewCount { get; set; }
    public bool IsResolved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? AuthorUsername { get; set; }
    public int ReplyCount { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class CreateQuestionRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? CropType { get; set; }
    public List<string>? Tags { get; set; }
}

public class UpdateQuestionRequestDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? CropType { get; set; }
    public bool? IsResolved { get; set; }
    public List<string>? Tags { get; set; }
}

public class QuestionQueryRequestDto : PagedRequest
{
    public string? CropType { get; set; }
    public string? Tag { get; set; }
    public bool? IsResolved { get; set; }
}

public class CommunityReplyDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsAccepted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? AuthorUsername { get; set; }
}

public class CreateReplyRequestDto
{
    public string Content { get; set; } = string.Empty;
}

public class UpdateReplyRequestDto
{
    public string? Content { get; set; }
}

public class QuestionDetailDto
{
    public CommunityQuestionDto Question { get; set; } = new();
    public List<CommunityReplyDto> Replies { get; set; } = new();
}

public class HotTopicDto
{
    public string CropType { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
}

public class HotTagDto
{
    public string Name { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
}
