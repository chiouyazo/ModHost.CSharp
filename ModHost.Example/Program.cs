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
		
		await bridge.RegisterCommandAsync("csharp", async context =>
		{
			await bridge.SendCommandFeedback(context.CommandContextId, $"Killed all players.");
			await bridge.ExecuteMinecraftCommandAsync("kill @a");
		});
		
		await bridge.RegisterCommandAsync("registerCommand", new List<CommandArgument>()
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
				await context.SendFeedbackAsync($"Trying to register command {context.Payload}.");
				string commandName = context.Payload.Split('=')[1];
				await context.ExecuteMinecraftCommandAsync($"say executed {commandName} {context.Payload}");
			});

		await bridge.RegisterCommandAsync("testTest", new List<CommandArgument>()
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
				await context.SendFeedbackAsync("Tester feedback");
				await context.ExecuteMinecraftCommandAsync($"say tttttttt {context.Payload}");
				await context.ExecuteMinecraftCommandAsync("say Test");
				CommandSource source = context.GetSource();
				await context.SendFeedbackAsync($"Was player: {await source.IsExecutedByPlayer()}");
				await context.SendFeedbackAsync($"Name: {await source.GetName()}");
			});
		
		Console.WriteLine("ModHost ready and connected.");

		await Task.Delay(-1);
	}
}