namespace ModHost.Models;

public class CommandSource
{
	private readonly ModHostBridge _bridge;
	private readonly string _commandId;
	private readonly string _platform;
	private readonly string _commandName;

	public CommandSource(ModHostBridge bridge, string commandId, string platform, string commandName)
	{
		_bridge = bridge;
		_commandId = commandId;
		_platform = platform;
		_commandName = commandName;
	}

	public async Task<bool> IsExecutedByPlayer()
	{
		string id = Guid.NewGuid().ToString();
		string response = await _bridge.SendRequestAsync(id, _platform, "COMMAND", "QUERY_COMMAND_SOURCE", $"{_commandId}:{_commandName}:IS_PLAYER");

		// Response could be: [id]:COMMAND_SOURCE_RESPONSE:[CommandContextId]:true
		string[] parts = response.Split(':');
		return parts.Length >= 6 && parts[5].Trim().ToLower() == "true";
	}

	public async Task<string?> GetName()
	{
		string id = Guid.NewGuid().ToString();
		string response = await _bridge.SendRequestAsync(id, _platform, "COMMAND", "QUERY_COMMAND_SOURCE", $"{_commandId}:{_commandName}:NAME");
        
		string[] parts = response.Split(':', 6);
		return parts.Length >= 6 ? parts[5] : null;
	}

	public async Task<bool> HasPermissionLevel(int level)
	{
		string id = Guid.NewGuid().ToString();
		string response = await _bridge.SendRequestAsync(id, _platform, "COMMAND", "QUERY_COMMAND_SOURCE", $"{_commandId}:{_commandName}:HASPERMISSIONLEVEL:{level}");
        
		string[] parts = response.Split(':', 6);
		
		if (parts.Length >= 6)
		{
			if (!bool.TryParse(parts[5], out bool result))
				return false;
			return result;
		}

		return false;
	}
}