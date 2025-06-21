using ModHost.Handlers;
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
		CommandHandler commandHandler = bridge.GetCommandHandler();

		await commandHandler.RegisterCommandAsync("csharp", async context =>
		{
			await commandHandler.SendCommandFeedback(context.CommandContextId, $"Killed all players.");
			await commandHandler.ExecuteMinecraftCommandAsync("kill @a");
		});
		
		await commandHandler.RegisterCommandAsync("registerCommand", new List<CommandArgument>()
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
				await context.SendFeedback($"Trying to register command {context.RawPayload}.");
				string commandName = context.RawPayload.Split('=')[1];
				await context.ExecuteMinecraftCommandAsync($"say executed {commandName} {context.RawPayload}");
			});

		await commandHandler.RegisterCommandAsync("testTest", new List<CommandArgument>()
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
				CommandSource source = context.GetSource();
				await context.SendFeedback($"Was player: {await source.IsExecutedByPlayer()}");
				await context.SendFeedback($"Name: {await source.GetName()}");
				await context.SendFeedback($"Has Permission level 0: {await source.HasPermissionLevel(0)}");
				await context.SendFeedback($"Has Permission level 1: {await source.HasPermissionLevel(1)}");
			},
			requirement: async context => await context.HasPermissionLevel(1));

		CommandHandler clientComm = bridge.GetClientCommandHandler();
		ScreenHandler screenHandler = bridge.GetScreenHandler();

		await clientComm.RegisterCommandAsync("meow",
			async context =>
			{
				await context.SendFeedback("Meow");
			});

		await clientComm.RegisterCommandAsync("screen", [
				new CommandArgument()
				{
					Name = "Action",
					Type = "string"
				},
				new CommandArgument()
				{
					IsOptional = true,
					Name = "Text",
					Type = "string"
				}
			], 
			async context =>
			{
				string placeholderText = "meow";

				if (context.Payload.TryGetValue("Text", out string? value))
					placeholderText = value;
				
				if (context.Payload["Action"] == "current")
				{
					await context.SendFeedback($"Current screen: {await screenHandler.CurrentScreen()}");
				}
				else if (context.Payload["Action"] == "new")
				{
					await context.SendFeedback($"Opening screen {await screenHandler.ShowNewScreen(placeholderText)}");
				}
				else if (context.Payload["Action"] == "push")
				{
					await context.SendFeedback("Unavailable.");
				}
				else if (context.Payload["Action"] == "showold")
				{
					await context.SendFeedback("Unavailable.");
				}
				else if (context.Payload["Action"] == "close")
				{
					await screenHandler.CloseScreen();
					await context.SendFeedback("Tried to close screen.");
				}
				else if (context.Payload["Action"] == "delete")
				{
					await context.SendFeedback("Unavailable.");
				}
				else if (context.Payload["Action"] == "list")
				{
					string[] screens = await screenHandler.ListScreens();
					await context.SendFeedback("Screens:");
					foreach (string screen in screens)
					{
						await context.SendFeedback($"- [{screen}]");
					}
				}
			});
		
		Console.WriteLine("ModHost ready and connected.");

		await Task.Delay(-1);
	}
}