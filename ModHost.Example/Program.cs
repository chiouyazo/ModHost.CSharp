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

		// await commandHandler.RegisterCommandAsync("csharp", async context =>
		// {
		// 	await commandHandler.SendCommandFeedback(context.CommandContextId, $"Killed all players.");
		// 	await commandHandler.ExecuteMinecraftCommandAsync("kill @a");
		// });
		//
		// await commandHandler.RegisterCommandAsync("registerCommand", new List<CommandArgument>()
		// 	{
		// 		new CommandArgument()
		// 		{
		// 			IsOptional = false,
		// 			Name = "Name",
		// 			Type = "string"
		// 		}
		// 	},
		// 	async context =>
		// 	{
		// 		// Returned payload: Name=1
		// 		await context.SendFeedback($"Trying to register command {context.RawPayload}.");
		// 		string commandName = context.RawPayload.Split('=')[1];
		// 		await context.ExecuteMinecraftCommandAsync($"say executed {commandName} {context.RawPayload}");
		// 	});
		//
		// await commandHandler.RegisterCommandAsync("testTest", new List<CommandArgument>()
		// 	{
		// 		new CommandArgument()
		// 		{
		// 			IsOptional = false,
		// 			Name = "Tester",
		// 			Type = "integer"
		// 		},
		// 		new CommandArgument()
		// 		{
		// 			IsOptional = true,
		// 			Name = "SecondTester",
		// 			Type = "integer"
		// 		}
		// 	},
		// 	async context =>
		// 	{
		// 		// Returned payload: Tester=1||SecondTester=1
		// 		await context.SendFeedback("Tester feedback");
		// 		CommandSource source = context.GetSource();
		// 		await context.SendFeedback($"Was player: {await source.IsExecutedByPlayer()}");
		// 		await context.SendFeedback($"Name: {await source.GetName()}");
		// 		await context.SendFeedback($"Has Permission level 0: {await source.HasPermissionLevel(0)}");
		// 		await context.SendFeedback($"Has Permission level 1: {await source.HasPermissionLevel(1)}");
		// 	},
		// 	requirement: async context => await context.HasPermissionLevel(1));

		CommandHandler clientComm = bridge.GetClientCommandHandler();

		CommandBuilder meowCommand = clientComm.CreateCommand("meow")
			.Executes(async context =>
			{
				await context.SendFeedback("Meow");
			});
		
		await clientComm.RegisterCommandAsync(meowCommand);
		
		ScreenHandler screenHandler = bridge.GetScreenHandler();
		
		CommandBuilder screenCommand = commandHandler.CreateCommand("screen");
		
		screenCommand.AddSubCommand(new CommandBuilder("current")
				.Executes(async context => 
				{
					await context.SendFeedback($"Current screen: {await screenHandler.CurrentScreen()}");
				}));

		screenCommand.AddSubCommand(new CommandBuilder("new")
				.AddArgument(new CommandArgument { Name = "Text", Type = "string", IsOptional = true })
				.Executes(async context =>
				{
					string placeholderText = "meow";
					if (context.Payload.TryGetValue("Text", out string? value))
						placeholderText = value;

					await context.SendFeedback($"Opening screen {await screenHandler.ShowNewScreen(placeholderText)}");
				}));
		
		screenCommand.AddSubCommand(new CommandBuilder("push")
				.Executes(async context =>
				{
					await context.SendFeedback("Unavailable.");
				}));
		
		screenCommand.AddSubCommand(new CommandBuilder("showold")
				.Executes(async context =>
				{
					await context.SendFeedback("Unavailable.");
				}));

		screenCommand.AddSubCommand(new CommandBuilder("close")
				.Executes(async context =>
				{
					await screenHandler.CloseScreen();
					await context.SendFeedback("Tried to close screen.");
				}));

		screenCommand.AddSubCommand(new CommandBuilder("delete")
				.Executes(async context =>
				{
					await context.SendFeedback("Unavailable.");
				}));

		screenCommand.AddSubCommand(new CommandBuilder("list")
				.Executes(async context =>
				{
					string[] screens = await screenHandler.ListScreens();
					await context.SendFeedback("Screens:");
					foreach (string screen in screens)
					{
						await context.SendFeedback($"- [{screen}]");
					}
				}));

		await commandHandler.RegisterCommandAsync(screenCommand);

		CommandBuilder dayCommand = commandHandler.CreateCommand("day")
			.Executes(async context =>
			{
				await context.ExecuteMinecraftCommandAsync("time set day");
				await context.ExecuteMinecraftCommandAsync("weather clear");
			})
			.Requires(async context => await context.HasPermissionLevel(1));
		
		await commandHandler.RegisterCommandAsync(dayCommand);
		
		Console.WriteLine("ModHost ready and connected.");

		await Task.Delay(-1);
	}
}