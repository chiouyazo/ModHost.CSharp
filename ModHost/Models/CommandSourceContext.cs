using ModHost.Handlers;

namespace ModHost.Models;

public class CommandSourceContext
{
	protected readonly string Platform;
	protected readonly string CommandName;
	protected readonly string Context;
	
	public string ContextId { get; private set; }
	
	protected readonly CommandHandler Handler;

	public CommandSourceContext(CommandHandler handler, string contextId, string platform, string commandName, string context)
	{
		Handler = handler;
		ContextId = contextId;
		Platform = platform;
		CommandName = commandName;
		Context = context;
	}
	
	private protected int SafeInt(string value)
	{
		if (int.TryParse(value, out int result))
			return result;
		return -1;
	}

	private protected double SafeDouble(string value)
	{
		if (double.TryParse(value, out double result))
			return result;
		return -1;
	}

	private protected float SafeFloat(string value)
	{
		if (float.TryParse(value, out float result))
			return result;
		return -1;
	}

	private protected bool SafeBool(string value)
	{
		if (bool.TryParse(value, out bool result))
			return result;
		return false;
	}

	private protected async Task<string> SendRequest(string query)
	{
		string id = Guid.NewGuid().ToString();
		string response = await Handler.Bridge.SendRequestAsync(id, Platform, "COMMAND", $"QUERY_{Context}_SOURCE", $"{ContextId}:{CommandName}:{query}");

		string[] parts = response.Split(':', 2);
		return parts.Length >= 2 ? parts[1] : string.Empty;
	}
}