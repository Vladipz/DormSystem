using MassTransit;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Data;
using Rooms.API.Entities;

using Shared.Data;

namespace Rooms.API.Services
{
    public class EventCreatedConsumer : IConsumer<EventCreated>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<EventCreatedConsumer> _logger;

        public EventCreatedConsumer(
            ApplicationDbContext dbContext,
            ILogger<EventCreatedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<EventCreated> context)
        {
            var message = context.Message;

            _logger.LogInformation(
                "Received EventCreated message for event {EventId} in building {BuildingId} and room {RoomId}",
                message.EventId,
                message.BuildingId,
                message.RoomId);

            // Check if we need to create a building-event relationship
            if (message.BuildingId.HasValue)
            {
                var building = await _dbContext.Buildings.FindAsync(message.BuildingId.Value);

                if (building != null)
                {
                    // Create a new BuildingEvent record
                    var buildingEvent = new BuildingEvent
                    {
                        BuildingId = message.BuildingId.Value,
                        EventId = message.EventId,
                        EventDate = message.Date,
                        EventName = message.Name,
                        IsPublic = message.IsPublic,
                    };

                    await _dbContext.BuildingEvents.AddAsync(buildingEvent);

                    _logger.LogInformation(
                        "Created BuildingEvent relationship for building {BuildingId} and event {EventId}",
                        message.BuildingId.Value,
                        message.EventId);

                    // If a room was specified, create a room-event relationship too
                    if (message.RoomId.HasValue)
                    {
                        var room = await _dbContext.Rooms
                            .Where(r => r.Id == message.RoomId.Value && r.BuildingId == message.BuildingId.Value)
                            .FirstOrDefaultAsync();

                        if (room != null)
                        {
                            var roomEvent = new RoomEvent
                            {
                                RoomId = message.RoomId.Value,
                                EventId = message.EventId,
                                EventDate = message.Date,
                                EventName = message.Name,
                            };

                            await _dbContext.RoomEvents.AddAsync(roomEvent);

                            _logger.LogInformation(
                                "Created RoomEvent relationship for room {RoomId} and event {EventId}",
                                message.RoomId.Value,
                                message.EventId);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Room {RoomId} not found in building {BuildingId} for event {EventId}",
                                message.RoomId.Value,
                                message.BuildingId.Value,
                                message.EventId);
                        }
                    }

                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    _logger.LogWarning(
                        "Building {BuildingId} not found for event {EventId}",
                        message.BuildingId.Value,
                        message.EventId);
                }
            }
        }
    }
}