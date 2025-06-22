using ModHost.Handlers;

namespace ModHost.Models.Server;

public class ServerCommandSource : CommandSource
{
	// ContextId is either commandId or providerId
	internal ServerCommandSource(CommandHandler handler, string contextId, string platform, string commandName, string context)
	: base(handler, contextId, platform, commandName, context) { }


	public ServerPlayerEntity GetPlayer()
	{
		return PlayerEntity.GetServerPlayer(Handler, ContextId, CommandName, Context);
	}
	
	public async Task SendMessage(string message)
	{
		await SendRequest($"SEND_MESSAGE:{message}");
	}
}