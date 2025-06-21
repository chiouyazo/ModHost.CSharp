using ModHost.Models;

namespace ModHost.Example;

public class Program
{
	public static async Task Main(string[] args)
	{
		if (args.Length == 0)
		{
			Console.WriteLine("Port argument required.");
			return;
		}

		if (!int.TryParse(args[0], out int port))
		{
			Console.WriteLine("Valid port required.");
			return;
		}

		ModHostBridge bridge = new ModHostBridge(port, "Csharp Example mod");
		
		await bridge.RegisterCommandAsync("csharp", "SERVER", async context =>
		{
			await bridge.SendCommandFeedback(context.CommandContextId, "SERVER", $"Killed all players.");
			await bridge.ExecuteMinecraftCommandAsync("kill @a", "SERVER");
		});
		
		await bridge.RegisterCommandAsync("registerCommand", "SERVER", new List<CommandArgument>()
			{
				new CommandArgument()
				{
					IsOptional = false,
					Name = "Name",
					Type = "string"
				}
			},
			async context =>
			{
				// Returned payload: Name=1
				await context.SendFeedback($"Trying to register command {context.Payload}.");
				string commandName = context.Payload.Split('=')[1];
				await context.ExecuteMinecraftCommandAsync($"say executed {commandName} {context.Payload}");
			});

		await bridge.RegisterCommandAsync("testTest", "SERVER", new List<CommandArgument>()
			{
				new CommandArgument()
				{
					IsOptional = false,
					Name = "Tester",
					Type = "integer"
				},
				new CommandArgument()
				{
					IsOptional = true,
					Name = "SecondTester",
					Type = "integer"
				}
			},
			async context =>
			{
				// Returned payload: Tester=1||SecondTester=1
				await context.SendFeedback("Tester feedback");
				await context.ExecuteMinecraftCommandAsync($"say tttttttt {context.Payload}");
				await context.ExecuteMinecraftCommandAsync("say Test");
				CommandSource source = context.GetSource();
				await context.SendFeedback($"Was player: {await source.IsExecutedByPlayer()}");
				await context.SendFeedback($"Name: {await source.GetName()}");
				await context.SendFeedback($"Has Permission level 0: {await source.HasPermissionLevel(0)}");
				await context.SendFeedback($"Has Permission level 1: {await source.HasPermissionLevel(1)}");
			},
			requirement: async context => await context.HasPermissionLevel(1));
		
		// await bridge.RegisterCommandAsync("meow", "CLIENT", [],
		// 	async context =>
		// 	{
		// 		await context.SendFeedback("Meow");
		// 	});
		
		Console.WriteLine("ModHost ready and connected.");

		await Task.Delay(-1);
	}
}