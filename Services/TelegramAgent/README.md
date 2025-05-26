# TelegramAgent Service

The TelegramAgent service is a microservice that delivers notifications via Telegram bot. It consumes `NotificationCreatedIntegrationEvent` messages from RabbitMQ and sends them to linked Telegram users.

## Features

- **Account Linking**: Users can link their web accounts to Telegram chats using 6-digit codes
- **Notification Delivery**: Automatically sends notifications to linked Telegram users
- **Bot Commands**: Supports `/start`, `/auth <code>`, and `/unlink` commands
- **Vertical Slice Architecture**: Each feature is self-contained in its own file

## Architecture

The service follows a vertical slice architecture where each business action is contained in a single file under `Features/`:

- `Features/AccountLinking.cs` - Handles bot commands and account linking
- `Features/NotificationDelivery.cs` - Consumes integration events and sends notifications
- `Data/TelegramDbContext.cs` - Entity Framework context for SQLite database
- `Entities/TelegramLink.cs` - Entity representing user-chat links

## Database Schema

The service uses SQLite with the following table:

```sql
CREATE TABLE TelegramLinks (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    ChatId BIGINT NOT NULL,
    LinkedAt DATETIME NOT NULL
);

-- Indexes
CREATE UNIQUE INDEX IX_TelegramLinks_UserId ON TelegramLinks (UserId);
CREATE UNIQUE INDEX IX_TelegramLinks_ChatId ON TelegramLinks (ChatId);
```

## Configuration

### Required Configuration

Update `appsettings.json` with your settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=TelegramAgent.db"
  },
  "TelegramBot": {
    "Token": "YOUR_BOT_TOKEN_HERE"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  },
  "Services": {
    "AuthService": {
      "BaseUrl": "http://localhost:5001"
    }
  }
}
```

### Getting a Telegram Bot Token

1. Message [@BotFather](https://t.me/botfather) on Telegram
2. Send `/newbot` command
3. Follow the instructions to create your bot
4. Copy the bot token and add it to your configuration

## Setup Instructions

### 1. Prerequisites

- .NET 8 SDK
- RabbitMQ server running
- Auth service running (for account linking)

### 2. Install Dependencies

```bash
cd Services/TelegramAgent/TelegramAgent.API
dotnet restore
```

### 3. Configure the Bot Token

Replace `YOUR_BOT_TOKEN_HERE` in `appsettings.json` with your actual bot token.

### 4. Run the Service

```bash
dotnet run
```

The service will:
- Create the SQLite database automatically
- Connect to RabbitMQ and create the `notification-created-consumer` queue
- Start the Telegram bot with long polling

## Usage

### Account Linking Flow

1. **Generate Code**: User generates a 6-digit code in the web application
2. **Start Bot**: User finds the bot on Telegram and sends `/start`
3. **Link Account**: User sends `/auth 123456` with their code
4. **Confirmation**: Bot confirms successful linking

### Bot Commands

- `/start` - Shows help message and instructions
- `/auth <code>` - Links account using 6-digit code (e.g., `/auth 123456`)
- `/unlink` - Unlinks the current account from this Telegram chat

### Notification Delivery

Once an account is linked:
1. NotificationCore publishes `NotificationCreatedIntegrationEvent` to RabbitMQ
2. TelegramAgent consumes the event
3. Service looks up the user's Telegram chat
4. Sends notification message to the chat

## Integration Events

### Consumed Events

**NotificationCreatedIntegrationEvent**
```csharp
public record NotificationCreatedIntegrationEvent(
    Guid NotificationId,
    Guid UserId,
    string Title,
    string Message,
    DateTime CreatedAt);
```

## API Dependencies

The service calls the Auth service to validate link codes:

**POST** `/api/link-codes/validate`
```json
{
  "code": "123456"
}
```

**Response:**
```json
{
  "userId": "guid-here"
}
```

## Error Handling

- **Invalid Codes**: Bot responds with helpful error messages
- **Telegram API Errors**: Logged but don't break the consumer
- **Missing Links**: Notifications are silently skipped for unlinked users
- **Auth Service Errors**: Gracefully handled with user-friendly messages

## Logging

The service uses structured logging with the following log levels:
- **Information**: Successful operations, account linking, notification delivery
- **Warning**: Invalid codes, failed validations
- **Error**: Telegram API errors, Auth service failures, unexpected exceptions

## Development

### Running in Development

```bash
# Set development environment
export ASPNETCORE_ENVIRONMENT=Development

# Run with hot reload
dotnet watch run
```

### Testing the Bot

1. Create a test bot with BotFather
2. Update `appsettings.Development.json` with the test bot token
3. Generate a link code in the Auth service
4. Test the `/auth` command with your bot

## Deployment

### Docker Support

The service can be containerized. Ensure the following in your deployment:

1. **Environment Variables**:
   - `TelegramBot__Token`
   - `RabbitMQ__Host`
   - `Services__AuthService__BaseUrl`

2. **Volumes**: Mount a volume for the SQLite database if persistence is needed

3. **Network**: Ensure connectivity to RabbitMQ and Auth service

### Production Considerations

- Use a dedicated RabbitMQ user instead of guest/guest
- Store the bot token securely (Azure Key Vault, etc.)
- Monitor the service logs for delivery failures
- Consider implementing health checks
- Set up proper backup for the SQLite database

## Troubleshooting

### Common Issues

1. **Bot not responding**: Check the bot token and ensure it's valid
2. **RabbitMQ connection failed**: Verify RabbitMQ is running and accessible
3. **Auth service unreachable**: Check the Auth service URL in configuration
4. **Database errors**: Ensure write permissions for SQLite file location

### Logs to Check

- MassTransit connection logs
- Telegram bot polling logs
- Account linking success/failure logs
- Notification delivery logs

## Security Notes

- Bot tokens should be kept secure and not committed to source control
- The service validates all input from Telegram users
- Database queries use parameterized queries to prevent injection
- HTTP calls to Auth service include proper error handling 