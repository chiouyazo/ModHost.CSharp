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
		
		ItemHandler itemHandler = bridge.GetItemHandler();
		BlockHandler blockHandler = bridge.GetBlockHandler();
		
		bool susItemResult = await itemHandler.RegisterItem("suspicious_substance", "NATURAL");
		
		if (!susItemResult)
			Console.WriteLine("Failed to register suspicious substance.");
		
		bool meowBlockResult = await blockHandler.RegisterBlock("meow_block", "SAND", true, "FUNCTIONAL");
		
		if (!meowBlockResult)
			Console.WriteLine("Failed to register meow block.");
		
		
		CommandHandler commandHandler = bridge.GetCommandHandler();
		CommandHandler clientComm = bridge.GetClientCommandHandler();

		CommandBuilder testDataCommand = commandHandler.CreateCommand("testdata")
			.AddArgument("name", "string", true, "player_names")
			.Executes(async context =>
			{
				await context.SendFeedback("Test feedback");
				CommandSource source = context.GetSource();
				await context.SendFeedback($"Was player: {await source.IsExecutedByPlayer()}");
				await context.SendFeedback($"Name: {await source.GetName()}");
				await context.SendFeedback($"Has Permission level 0: {await source.HasPermissionLevel(0)}");
				await context.SendFeedback($"Has Permission level 1: {await source.HasPermissionLevel(1)}");
			});

		await commandHandler.RegisterCommandAsync(testDataCommand);
		
		commandHandler.RegisterSuggestionProvider("player_names", async (completion, ctx) =>
		{
			List<string> playerNames = await ctx.GetPlayerNames();
			
			return playerNames;
			// return list.Where(x => x.StartsWith(completion));
			// await commandHandler.FinalizeSuggestion(ctx.ContextId); // TODO: This has to be done after the response was send
		});
		
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