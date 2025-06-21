using ModHost.Handlers;

namespace ModHost.Models;

public class CommandSource
{
	private readonly CommandHandler _handler;
	private readonly string _commandId;
	private readonly string _platform;
	private readonly string _commandName;

	public CommandSource(CommandHandler handler, string commandId, string platform, string commandName)
	{
		_handler = handler;
		_commandId = commandId;
		_platform = platform;
		_commandName = commandName;
	}

	public async Task<bool> IsExecutedByPlayer()
	{
		string id = Guid.NewGuid().ToString();
		string response = await _handler.Bridge.SendRequestAsync(id, _platform, "COMMAND", "QUERY_COMMAND_SOURCE", $"{_commandId}:{_commandName}:IS_PLAYER");

		// Response could be: [CommandContextId]:true
		string[] parts = response.Split(':');
		return parts.Length >= 2 && parts[1].Trim().ToLower() == "true";
	}

	public async Task<string?> GetName()
	{
		string id = Guid.NewGuid().ToString();
		string response = await _handler.Bridge.SendRequestAsync(id, _platform, "COMMAND", "QUERY_COMMAND_SOURCE", $"{_commandId}:{_commandName}:NAME");
        
		string[] parts = response.Split(':', 2);
		return parts.Length >= 2 ? parts[1] : null;
	}

	public async Task<bool> HasPermissionLevel(int level)
	{
		string id = Guid.NewGuid().ToString();
		string response = await _handler.Bridge.SendRequestAsync(id, _platform, "COMMAND", "QUERY_COMMAND_SOURCE", $"{_commandId}:{_commandName}:HASPERMISSIONLEVEL:{level}");
        
		string[] parts = response.Split(':', 2);
		
		if (parts.Length >= 2)
		{
			if (!bool.TryParse(parts[1], out bool result))
				return false;
			return result;
		}

		return false;
	}
}