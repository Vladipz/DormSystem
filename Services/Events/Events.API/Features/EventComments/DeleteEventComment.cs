using Carter;
using Carter.OpenApi;

using ErrorOr;

using Events.API.Database;
using Events.API.Mappins;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Shared.TokenService.Services;

namespace Events.API.Features.EventComments
{
    public static class DeleteEventComment
    {
        internal sealed class Command : IRequest<ErrorOr<bool>>
        {
            public Guid EventId { get; set; }

            public Guid CommentId { get; set; }

            public Guid CurrentUserId { get; set; }

            public bool IsAdmin { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.EventId).NotEmpty();
                RuleFor(x => x.CommentId).NotEmpty();
                RuleFor(x => x.CurrentUserId).NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<bool>>
        {
            private readonly EventsDbContext _eventDbContext;
            private readonly IValidator<Command> _validator;

            public Handler(EventsDbContext eventDbContext, IValidator<Command> validator)
            {
                _eventDbContext = eventDbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<bool>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return validationResult.ToValidationError<bool>();
                }

                var comment = await _eventDbContext.EventComments
                    .Include(c => c.Event)
                    .Where(c => c.Id == request.CommentId)
                    .Where(c => c.EventId == request.EventId)
                    .Where(c => c.DeletedAt == null)
                    .FirstOrDefaultAsync(cancellationToken);

                if (comment is null)
                {
                    return Error.NotFound("EventComment.NotFound", "The specified comment was not found.");
                }

                var canDelete = request.IsAdmin
                    || comment.AuthorUserId == request.CurrentUserId
                    || comment.Event.OwnerId == request.CurrentUserId;

                if (!canDelete)
                {
                    return Error.Forbidden("EventComments.Forbidden", "You cannot delete this comment.");
                }

                comment.DeletedAt = DateTime.UtcNow;
                await _eventDbContext.SaveChangesAsync(cancellationToken);

                return true;
            }
        }
    }

    public sealed class DeleteEventCommentEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/events/{eventId}/comments/{commentId}", async (
                Guid eventId,
                Guid commentId,
                ISender sender,
                HttpContext httpContext,
                ITokenService tokenService) =>
            {
                var userIdResult = tokenService.GetUserId(httpContext);
                if (userIdResult.IsError)
                {
                    return Results.Unauthorized();
                }

                var command = new DeleteEventComment.Command
                {
                    EventId = eventId,
                    CommentId = commentId,
                    CurrentUserId = userIdResult.Value,
                    IsAdmin = httpContext.User.IsInRole("Admin"),
                };

                var result = await sender.Send(command);

                return result.Match(
                    success => Results.NoContent(),
                    errors => errors.ToResponse());
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces<Error>(StatusCodes.Status400BadRequest)
            .Produces<Error>(StatusCodes.Status401Unauthorized)
            .Produces<Error>(StatusCodes.Status403Forbidden)
            .Produces<Error>(StatusCodes.Status404NotFound)
            .WithTags("EventComments")
            .WithName("DeleteEventComment")
            .IncludeInOpenApi()
            .RequireAuthorization();
        }
    }
}
