using ModHost.Handlers;

namespace ModHost.Models.Server;

public class ServerPlayerEntity : PlayerEntity
{
	internal ServerPlayerEntity(CommandHandler handler, string contextId, string platform, string commandName, string context)
		: base(handler, contextId, platform, commandName, context) { }
	
	public async Task<bool> NotInAnyWorld()
	{
		return SafeBool(await SendRequest("PLAYER_NOT_IN_ANY_WORLD"));
	}
	
	public async Task<bool> SeenCredits()
	{
		return SafeBool(await SendRequest("PLAYER_SEEN_CREDITS"));
	}
}