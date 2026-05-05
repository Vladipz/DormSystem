using Carter;
using Carter.OpenApi;

using ErrorOr;

using Events.API.Contracts;
using Events.API.Database;
using Events.API.Entities;
using Events.API.Mappins;
using Events.API.Services;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Shared.TokenService.Services;

namespace Events.API.Features.EventComments
{
    public static class CreateEventComment
    {
        internal sealed class Command : IRequest<ErrorOr<EventCommentResponse>>
        {
            public Guid EventId { get; set; }

            public Guid AuthorUserId { get; set; }

            public bool IsAdmin { get; set; }

            public string Content { get; set; } = string.Empty;
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.EventId).NotEmpty();
                RuleFor(x => x.AuthorUserId).NotEmpty();
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

                var eventAccess = await _eventDbContext.Events
                    .AsNoTracking()
                    .Where(e => e.Id == request.EventId)
                    .Select(e => new
                    {
                        e.Id,
                        e.OwnerId,
                        IsParticipant = e.Participants.Any(p => p.UserId == request.AuthorUserId),
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (eventAccess is null)
                {
                    return Error.NotFound("Event.NotFound", "The specified event was not found.");
                }

                var canComment = request.IsAdmin || eventAccess.OwnerId == request.AuthorUserId || eventAccess.IsParticipant;
                if (!canComment)
                {
                    return Error.Forbidden("EventComments.Forbidden", "You must attend this event before commenting.");
                }

                var comment = new EventComment
                {
                    Id = Guid.NewGuid(),
                    EventId = request.EventId,
                    AuthorUserId = request.AuthorUserId,
                    Content = request.Content,
                    CreatedAt = DateTime.UtcNow,
                };

                _eventDbContext.EventComments.Add(comment);
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

    public sealed class CreateEventCommentEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/events/{eventId}/comments", async (
                Guid eventId,
                CreateEventCommentRequest request,
                ISender sender,
                HttpContext httpContext,
                ITokenService tokenService) =>
            {
                var userIdResult = tokenService.GetUserId(httpContext);
                if (userIdResult.IsError)
                {
                    return Results.Unauthorized();
                }

                var command = new CreateEventComment.Command
                {
                    EventId = eventId,
                    AuthorUserId = userIdResult.Value,
                    Content = request.Content,
                    IsAdmin = httpContext.User.IsInRole("Admin"),
                };

                var result = await sender.Send(command);

                return result.Match(
                    success => Results.Created($"/events/{eventId}/comments/{success.Id}", success),
                    errors => errors.ToResponse());
            })
            .Produces<EventCommentResponse>(StatusCodes.Status201Created)
            .Produces<Error>(StatusCodes.Status400BadRequest)
            .Produces<Error>(StatusCodes.Status401Unauthorized)
            .Produces<Error>(StatusCodes.Status403Forbidden)
            .Produces<Error>(StatusCodes.Status404NotFound)
            .WithTags("EventComments")
            .WithName("CreateEventComment")
            .Accepts<CreateEventCommentRequest>("application/json")
            .IncludeInOpenApi()
            .RequireAuthorization();
        }
    }
}
