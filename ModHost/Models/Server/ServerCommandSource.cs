using ModHost.Handlers;

namespace ModHost.Models.Server;

public class ServerCommandSource : CommandSource
{
	// ContextId is either commandId or providerId
	internal ServerCommandSource(CommandHandler handler, string contextId, string platform, string commandName, string context)
	: base(handler, contextId, platform, commandName, context) { }

	public async Task SendMessage(string message)
	{
		string id = Guid.NewGuid().ToString();
		await _handler.Bridge.SendRequestAsync(id, _platform, "COMMAND", $"QUERY_{_context}_SOURCE", $"{ContextId}:{_commandName}:SEND_MESSAGE:{message}");
	}
}