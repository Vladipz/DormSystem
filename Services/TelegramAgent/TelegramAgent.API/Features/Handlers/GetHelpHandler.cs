using MediatR;
using TelegramAgent.API.Features.Queries.GetHelp;

namespace TelegramAgent.API.Features.Handlers
{
    public class GetHelpHandler : IRequestHandler<GetHelpQuery, HelpResult>
    {
        private readonly ILogger<GetHelpHandler> _logger;

        public GetHelpHandler(ILogger<GetHelpHandler> logger)
        {
            _logger = logger;
        }

        public Task<HelpResult> Handle(GetHelpQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Help requested");

            var helpText = """
                Welcome to the Dorm System Notification Bot! 🏠
                
                Available commands:
                /start or /help - Show this help message
                /auth <code> - Link your account using a 6-digit code
                /unlink - Unlink your account from this chat
                /status - Check your current link status
                
                📱 To link your account:
                1. Go to your account settings in the web app
                2. Generate a link code
                3. Send: /auth 123456 (replace with your actual code)
                
                ✅ Once linked, you'll receive notifications about events and updates.
                ❌ To stop receiving notifications, use /unlink.
                
                Need more help? Contact support through the web application.
                """;

            return Task.FromResult(new HelpResult(helpText));
        }
    }
}