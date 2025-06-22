using ModHost.Handlers;
using ModHost.Models.Server;

namespace ModHost.Models;

public abstract class CommandSource : CommandSourceContext
{
	internal CommandSource(CommandHandler handler, string contextId, string platform, string commandName, string context) : base(handler, contextId, platform, commandName, context) { }

	public static ServerCommandSource GetServerContext(CommandHandler handler, string contextId, string commandName, string context)
	{
		return new ServerCommandSource(handler, contextId, "SERVER", commandName, context);
	}

	public async Task<bool> IsExecutedByPlayer()
	{
		return SafeBool(await SendRequest("IS_PLAYER"));
	}

	public async Task<string?> GetName()
	{
		return await SendRequest("NAME");
	}

	public async Task<bool> HasPermissionLevel(int level)
	{
		return SafeBool(await SendRequest($"HASPERMISSIONLEVEL:{level}"));
	}

	public async Task<string> DisplayName()
	{
		return await SendRequest("DISPLAYNAME");
	}

	public async Task<bool> IsSilent()
	{
		return SafeBool(await SendRequest("ISSILENT"));
	}

	public async Task SendError(string message)
	{
		await SendRequest($"SEND_ERROR:{message}");
	}

	public async Task SendFeedback(string message)
	{
		await SendRequest($"SEND_FEEDBACK:{message}");
	}
}