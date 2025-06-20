namespace ModHost.Models;

public class CommandSource
{
	private readonly ModHostBridge _bridge;
	private readonly string _commandId;

	public CommandSource(ModHostBridge bridge, string commandId)
	{
		_bridge = bridge;
		_commandId = commandId;
	}

	public async Task<bool> IsExecutedByPlayer()
	{
		string id = Guid.NewGuid().ToString();
		string response = await _bridge.SendRequestAsync(id, "QUERY_COMMAND_SOURCE", $"{_commandId}:IS_PLAYER");

		// Response could be: [id]:COMMAND_SOURCE_RESPONSE:[CommandContextId]:true
		string[] parts = response.Split(':');
		return parts.Length >= 4 && parts[3].Trim().ToLower() == "true";
	}

	public async Task<string?> GetName()
	{
		string id = Guid.NewGuid().ToString();
		string response = await _bridge.SendRequestAsync(id, "QUERY_COMMAND_SOURCE", $"{_commandId}:NAME");
        
		string[] parts = response.Split(':', 4);
		return parts.Length >= 4 ? parts[3] : null;
	}
}