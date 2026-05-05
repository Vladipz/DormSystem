using Carter;
using Carter.OpenApi;

using ErrorOr;

using Events.API.Contracts;
using Events.API.Database;
using Events.API.Mappins;
using Events.API.Services;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Shared.TokenService.Services;

namespace Events.API.Features.EventComments
{
    public static class UpdateEventComment
    {
        internal sealed class Command : IRequest<ErrorOr<EventCommentResponse>>
        {
            public Guid EventId { get; set; }

            public Guid CommentId { get; set; }

            public Guid CurrentUserId { get; set; }

            public bool IsAdmin { get; set; }

            public string Content { get; set; } = string.Empty;
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.EventId).NotEmpty();
                RuleFor(x => x.CommentId).NotEmpty();
                RuleFor(x => x.CurrentUserId).NotEmpty();
                RuleFor(x => x.Content)
                    .Must(content => !string.IsNullOrWhiteSpace(content))
                    .WithMessage("Comment content is required.")
                    .MaximumLength(2000)
                    .WithMessage("Comment content cannot exceed 2000 characters.");
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<EventCommentResponse>>
        {
            private readonly EventsDbContext _eventDbContext;
            private readonly IValidator<Command> _validator;
            private readonly CommentAuthorEnricher _commentAuthorEnricher;

            public Handler(
                EventsDbContext eventDbContext,
                IValidator<Command> validator,
                CommentAuthorEnricher commentAuthorEnricher)
            {
                _eventDbContext = eventDbContext;
                _validator = validator;
                _commentAuthorEnricher = commentAuthorEnricher;
            }

            public async Task<ErrorOr<EventCommentResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                request.Content = request.Content.Trim();

                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return validationResult.ToValidationError<EventCommentResponse>();
                }

                var comment = await _eventDbContext.EventComments
                    .Where(c => c.Id == request.CommentId)
                    .Where(c => c.EventId == request.EventId)
                    .Where(c => c.DeletedAt == null)
                    .FirstOrDefaultAsync(cancellationToken);

                if (comment is null)
                {
                    return Error.NotFound("EventComment.NotFound", "The specified comment was not found.");
                }

                if (!request.IsAdmin && comment.AuthorUserId != request.CurrentUserId)
                {
                    return Error.Forbidden("EventComments.Forbidden", "You cannot edit this comment.");
                }

                comment.Content = request.Content;
                comment.UpdatedAt = DateTime.UtcNow;

                await _eventDbContext.SaveChangesAsync(cancellationToken);

                var response = new EventCommentResponse
                {
                    Id = comment.Id,
                    EventId = comment.EventId,
                    AuthorUserId = comment.AuthorUserId,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt,
                    UpdatedAt = comment.UpdatedAt,
                };

                await _commentAuthorEnricher.EnrichAuthorsAsync(new[] { response });

                return response;
            }
        }
    }

    public sealed class UpdateEventCommentEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/events/{eventId}/comments/{commentId}", async (
                Guid eventId,
                Guid commentId,
                UpdateEventCommentRequest request,
                ISender sender,
                HttpContext httpContext,
                ITokenService tokenService) =>
            {
                var userIdResult = tokenService.GetUserId(httpContext);
                if (userIdResult.IsError)
                {
                    return Results.Unauthorized();
                }

                var command = new UpdateEventComment.Command
                {
                    EventId = eventId,
                    CommentId = commentId,
                    CurrentUserId = userIdResult.Value,
                    Content = request.Content,
                    IsAdmin = httpContext.User.IsInRole("Admin"),
                };

                var result = await sender.Send(command);

                return result.Match(
                    success => Results.Ok(success),
                    errors => errors.ToResponse());
            })
            .Produces<EventCommentResponse>(StatusCodes.Status200OK)
            .Produces<Error>(StatusCodes.Status400BadRequest)
            .Produces<Error>(StatusCodes.Status401Unauthorized)
            .Produces<Error>(StatusCodes.Status403Forbidden)
            .Produces<Error>(StatusCodes.Status404NotFound)
            .WithTags("EventComments")
            .WithName("UpdateEventComment")
            .Accepts<UpdateEventCommentRequest>("application/json")
            .IncludeInOpenApi()
            .RequireAuthorization();
        }
    }
}
