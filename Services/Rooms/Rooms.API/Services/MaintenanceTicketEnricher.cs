using ErrorOr;

using Rooms.API.Contracts.Maintenance;
using Rooms.API.Entities;

using Shared.UserServiceClient;

namespace Rooms.API.Services
{
    public class MaintenanceTicketEnricher
    {
        private readonly IAuthServiceClient _authService;
        private readonly ILogger<MaintenanceTicketEnricher> _logger;

        public MaintenanceTicketEnricher(IAuthServiceClient authService, ILogger<MaintenanceTicketEnricher> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Enriches a collection of maintenance tickets with user information.
        /// </summary>
        public async Task<List<MaintenanceTicketResponse>> EnrichMaintenanceTicketsAsync(List<MaintenanceTicketResponse> tickets)
        {
            ArgumentNullException.ThrowIfNull(tickets);

            if (tickets.Count == 0)
            {
                return new List<MaintenanceTicketResponse>();
            }

            // Collect all user IDs to retrieve in a single batch
            var userIds = new HashSet<Guid>();
            foreach (var ticket in tickets)
            {
                if (ticket.Reporter != null)
                {
                    userIds.Add(ticket.Reporter.Id);
                }

                if (ticket.AssignedTo != null)
                {
                    userIds.Add(ticket.AssignedTo.Id);
                }
            }

            // Get all users in one call
            var usersResult = await _authService.GetUsersByIdsAsync(userIds);
            if (usersResult.IsError)
            {
                _logger.LogWarning("Failed to get user information: {Error}", usersResult.FirstError.Description);
                return tickets;
            }

            var users = usersResult.Value;
            var enrichedTickets = tickets.Select(ticket => new MaintenanceTicketResponse
            {
                Id = ticket.Id,
                Room = ticket.Room,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                CreatedAt = ticket.CreatedAt,
                ResolvedAt = ticket.ResolvedAt,

                // Якщо користувач знайдений - використовуємо дані з сервісу, якщо ні - залишаємо оригінальні
                Reporter = users.TryGetValue(ticket.Reporter?.Id ?? Guid.Empty, out var reporter)
                    ? new UserDto
                    {
                        Id = reporter.Id,
                        FirstName = reporter.FirstName,
                        LastName = reporter.LastName,
                        Email = reporter.Email,
                    }
                    : ticket.Reporter,

                // Те саме для AssignedTo
                AssignedTo = ticket.AssignedTo != null && users.TryGetValue(ticket.AssignedTo.Id, out var assignedTo)
                    ? new UserDto
                    {
                        Id = assignedTo.Id,
                        FirstName = assignedTo.FirstName,
                        LastName = assignedTo.LastName,
                        Email = assignedTo.Email,
                    }
                    : ticket.AssignedTo,
                Priority = ticket.Priority,
            }).ToList();

            return enrichedTickets;
        }

        /// <summary>
        /// Enriches a single maintenance ticket with user information.
        /// </summary>
        // public async Task<MaintenanceTicketEnrichedResponse> EnrichMaintenanceTicketAsync(MaintenanceTicketResponse ticket)
        // {
        //     ArgumentNullException.ThrowIfNull(ticket);

        //     var enrichedTicket = new MaintenanceTicketEnrichedResponse
        //     {
        //         Id = ticket.Id,
        //         Room = ticket.Room,
        //         Title = ticket.Title,
        //         Description = ticket.Description,
        //         Status = ticket.Status,
        //         CreatedAt = ticket.CreatedAt,
        //         ResolvedAt = ticket.ResolvedAt,
        //         ReporterById = ticket.ReporterById,
        //         AssignedToId = ticket.AssignedToId,
        //         Priority = ticket.Priority
        //     };

        //     // Get reporter details
        //     var reporterResult = await _authService.GetUserByIdAsync(ticket.ReporterById);
        //     if (!reporterResult.IsError)
        //     {
        //         var reporter = reporterResult.Value;
        //         enrichedTicket.ReporterFirstName = reporter.FirstName;
        //         enrichedTicket.ReporterLastName = reporter.LastName;
        //         enrichedTicket.ReporterEmail = reporter.Email;
        //     }
        //     else
        //     {
        //         _logger.LogWarning(
        //             "Failed to get reporter information for ticket {TicketId}, user {UserId}: {Error}",
        //             ticket.Id,
        //             ticket.ReporterById,
        //             reporterResult.FirstError.Description);
        //     }

        //     // Get assignee details if assigned
        //     if (ticket.AssignedToId.HasValue)
        //     {
        //         var assigneeResult = await _authService.GetUserByIdAsync(ticket.AssignedToId.Value);
        //         if (!assigneeResult.IsError)
        //         {
        //             var assignee = assigneeResult.Value;
        //             enrichedTicket.AssignedToFirstName = assignee.FirstName;
        //             enrichedTicket.AssignedToLastName = assignee.LastName;
        //             enrichedTicket.AssignedToEmail = assignee.Email;
        //         }
        //         else
        //         {
        //             _logger.LogWarning(
        //                 "Failed to get assignee information for ticket {TicketId}, user {UserId}: {Error}",
        //                 ticket.Id,
        //                 ticket.AssignedToId.Value,
        //                 assigneeResult.FirstError.Description);
        //         }
        //     }

        //     return enrichedTicket;
        // }
    }
}