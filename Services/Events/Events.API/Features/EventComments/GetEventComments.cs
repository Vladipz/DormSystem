using Carter;
using Carter.OpenApi;

using ErrorOr;

using Events.API.Contracts;
using Events.API.Database;
using Events.API.Mappins;
using Events.API.Services;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Shared.PagedList;
using Shared.TokenService.Services;

namespace Events.API.Features.EventComments
{
    public static class GetEventComments
    {
        internal sealed class Query : IRequest<ErrorOr<PagedResponse<EventCommentResponse>>>
        {
            public Guid EventId { get; set; }

            public int PageNumber { get; set; } = 1;

            public int PageSize { get; set; } = 3;

            public Guid? CurrentUserId { get; set; }

            public bool IsAdmin { get; set; }
        }

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<PagedResponse<EventCommentResponse>>>
        {
            private readonly EventsDbContext _eventDbContext;
            private readonly CommentAuthorEnricher _commentAuthorEnricher;

            public Handler(EventsDbContext eventDbContext, CommentAuthorEnricher commentAuthorEnricher)
            {
                _eventDbContext = eventDbContext;
                _commentAuthorEnricher = commentAuthorEnricher;
            }

            public async Task<ErrorOr<PagedResponse<EventCommentResponse>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var eventAccess = await _eventDbContext.Events
                    .AsNoTracking()
                    .Where(e => e.Id == request.EventId)
                    .Select(e => new
                    {
                        e.Id,
                        e.OwnerId,
                        e.IsPublic,
                        IsParticipant = request.CurrentUserId.HasValue && e.Participants.Any(p => p.UserId == request.CurrentUserId.Value),
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (eventAccess is null)
                {
                    return Error.NotFound("Event.NotFound", "The specified event was not found.");
                }

                var canRead = eventAccess.IsPublic
                    || request.IsAdmin
                    || (request.CurrentUserId.HasValue && eventAccess.OwnerId == request.CurrentUserId.Value)
                    || eventAccess.IsParticipant;

                if (!canRead)
                {
                    return request.CurrentUserId.HasValue
                        ? Error.Forbidden("EventComments.Forbidden", "You cannot view comments for this event.")
                        : Error.Unauthorized("EventComments.Unauthorized", "Authentication is required to view comments for this event.");
                }

                var commentsQuery = _eventDbContext.EventComments
                    .AsNoTracking()
                    .Where(comment => comment.EventId == request.EventId)
                    .Where(comment => comment.DeletedAt == null)
                    .OrderByDescending(comment => comment.CreatedAt)
                    .ThenByDescending(comment => comment.Id)
                    .Select(comment => new EventCommentResponse
                    {
                        Id = comment.Id,
                        EventId = comment.EventId,
                        AuthorUserId = comment.AuthorUserId,
                        Content = comment.Content,
                        CreatedAt = comment.CreatedAt,
                        UpdatedAt = comment.UpdatedAt,
                    });

                var pagedComments = await PagedList<EventCommentResponse>.CreateAsync(
                    commentsQuery,
                    request.PageNumber,
                    request.PageSize,
                    cancellationToken);

                var response = PagedResponse<EventCommentResponse>.FromPagedList(pagedComments);
                await _commentAuthorEnricher.EnrichAuthorsAsync(response.Items);

                return response;
            }
        }
    }

    public sealed class GetEventCommentsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/events/{eventId}/comments", async (
                Guid eventId,
                ISender sender,
                HttpContext httpContext,
                ITokenService tokenService,
                int pageNumber = 1,
                int pageSize = 3) =>
            {
                var userIdResult = tokenService.GetUserId(httpContext);

                var query = new GetEventComments.Query
                {
                    EventId = eventId,
                    PageNumber = Math.Max(1, pageNumber),
                    PageSize = Math.Clamp(pageSize, 1, 50),
                    CurrentUserId = userIdResult.IsError ? null : userIdResult.Value,
                    IsAdmin = httpContext.User.IsInRole("Admin"),
                };

                var result = await sender.Send(query);

                return result.Match(
                    success => Results.Ok(success),
                    errors => errors.ToResponse());
            })
            .Produces<PagedResponse<EventCommentResponse>>(StatusCodes.Status200OK)
            .Produces<Error>(StatusCodes.Status401Unauthorized)
            .Produces<Error>(StatusCodes.Status403Forbidden)
            .Produces<Error>(StatusCodes.Status404NotFound)
            .WithTags("EventComments")
            .WithName("GetEventComments")
            .WithSummary("Get paginated event comments")
            .IncludeInOpenApi();
        }
    }
}
