using ModHost.Handlers;
using ModHost.Models.Communication;
using ModHost.Models.Server;

namespace ModHost.Models;

public class CommandContext
{
	private readonly CommandHandler _handler;
	private readonly string _platform;

	public bool Finalized { get; private set; }
	public string CommandContextId { get; }
	public string CommandName { get; }
	public string RawPayload { get; }

	public Dictionary<string, string> Payload { get; set; } = new Dictionary<string, string>();


	public CommandContext(CommandHandler handler, MessageBase message, string commandName, string rawPayload)
	{
		_handler = handler;
		_platform = message.Platform;
		CommandContextId = message.Id;
		RawPayload = rawPayload;
		CommandName = commandName;
		
		string[] arguments = rawPayload.Split("||");
		if (!arguments.Any())
			return;
		
		foreach (string argument in arguments)
		{
			if(!argument.Contains("="))
				continue;
			string[] strings = argument.Split("=", 2);
			Payload.Add(strings[0], strings[1]);
		}
	}
	
	public ServerCommandSource GetSource()
	{
		return new ServerCommandSource(_handler, CommandContextId, _platform, CommandName, "COMMAND");
	}
	
	public async Task SendFeedback(string message)
	{
		if (Finalized)
			throw new InvalidOperationException("Cannot send feedback after finalizing command.");

		await _handler.SendCommandFeedback(CommandContextId, message);
	}

	public async Task ExecuteMinecraftCommandAsync(string command)
	{
		await _handler.ExecuteMinecraftCommandAsync(command);
	}

	public async Task FinalizeAsync()
	{
		if (Finalized) 
			return;
		
		Finalized = true;
		await _handler.FinalizeCommand(CommandContextId);
	}
}