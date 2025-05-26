using TelegramAgent.API.Features.Bot.Commands;

namespace TelegramAgent.API.Features.Bot
{
    public interface ITelegramCommandRegistry
    {
        ITelegramCommand? GetCommand(string commandName);
        IEnumerable<ITelegramCommand> GetAllCommands();
    }

    public class TelegramCommandRegistry : ITelegramCommandRegistry
    {
        private readonly Dictionary<string, ITelegramCommand> _commands;
        private readonly ILogger<TelegramCommandRegistry> _logger;

        public TelegramCommandRegistry(IEnumerable<ITelegramCommand> commands, ILogger<TelegramCommandRegistry> logger)
        {
            _logger = logger;
            _commands = new Dictionary<string, ITelegramCommand>(StringComparer.OrdinalIgnoreCase);

            foreach (var command in commands)
            {
                _commands[command.CommandName] = command;
                _logger.LogInformation("Registered Telegram command: {CommandName}", command.CommandName);
            }

            _logger.LogInformation("Total registered commands: {Count}", _commands.Count);
        }

        public ITelegramCommand? GetCommand(string commandName)
        {
            _commands.TryGetValue(commandName, out var command);
            return command;
        }

        public IEnumerable<ITelegramCommand> GetAllCommands()
        {
            return _commands.Values;
        }
    }
}