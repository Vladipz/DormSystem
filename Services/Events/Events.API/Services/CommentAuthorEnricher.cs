using Events.API.Contracts;

using Shared.UserServiceClient;

namespace Events.API.Services
{
    public sealed class CommentAuthorEnricher
    {
        private readonly IAuthServiceClient _authService;
        private readonly ILogger<CommentAuthorEnricher> _logger;

        public CommentAuthorEnricher(IAuthServiceClient authService, ILogger<CommentAuthorEnricher> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task EnrichAuthorsAsync(ICollection<EventCommentResponse> comments)
        {
            ArgumentNullException.ThrowIfNull(comments);

            if (comments.Count == 0)
            {
                return;
            }

            var authorIds = comments
                .Select(comment => comment.AuthorUserId)
                .Distinct()
                .ToList();

            var usersResult = await _authService.GetUsersByIdsAsync(authorIds);

            if (usersResult.IsError)
            {
                _logger.LogWarning(
                    "Failed to enrich event comment authors: {Error}",
                    usersResult.FirstError.Description);
                return;
            }

            foreach (var comment in comments)
            {
                if (usersResult.Value.TryGetValue(comment.AuthorUserId, out var author))
                {
                    comment.Author = author;
                }
            }
        }
    }
}
