namespace ModHost.Models;

public class CommandContext
{
	private readonly ModHostBridge _bridge;
	
	public bool Finalized { get; private set; }
	public string CommandContextId { get; }
	public string Payload { get; }


	public CommandContext(ModHostBridge bridge, string commandContextId, string payload)
	{
		_bridge = bridge;
		CommandContextId = commandContextId;
		Payload = payload;
	}
	
	public CommandSource GetSource()
	{
		return new CommandSource(_bridge, CommandContextId);
	}
	
	public async Task SendFeedbackAsync(string message)
	{
		if (Finalized)
			throw new InvalidOperationException("Cannot send feedback after finalizing command.");

		await _bridge.SendCommandFeedback(CommandContextId, message);
	}

	public async Task ExecuteMinecraftCommandAsync(string command)
	{
		await _bridge.ExecuteMinecraftCommandAsync(command);
	}

	public async Task FinalizeAsync()
	{
		if (Finalized) 
			return;
		
		Finalized = true;
		await _bridge.FinalizeCommand(CommandContextId);
	}
}