using ModHost.Handlers;
using ModHost.Models;
using ModHost.Models.Communication.Items;

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
		
		// bool susItemResult = await itemHandler.RegisterItem("suspicious_substance", "NATURAL");
		
		// if (!susItemResult)
			// Console.WriteLine("Failed to register suspicious substance.");

		DefenseMap defenseMap = new DefenseMap(1, 1, 1, 1, 1);
		ArmorPayload armorPayload = new ArmorPayload(2, defenseMap, 2, "block.anvil.destroy", 0.0f, 0.0f, "repairs_leather_armor",
			"suspicious_helmet");

		ItemRegistration itemRegistration =
			new ItemRegistration("suspicious_helmet", ItemType.Armor, armorPayload, "natural_blocks");
		itemRegistration.SetFood(new FoodPayload(3, 3, true));
		
		bool armorResult = await itemHandler.RegisterItem(itemRegistration);
			
		if(armorResult)
			Console.WriteLine("Armor has been registered successfully");
		else
			Console.WriteLine("Could not register armor");
		
		bool meowBlockResult = await blockHandler.RegisterBlock("meow_block", "SAND", true, "functional_blocks");
		
		if (!meowBlockResult)
			Console.WriteLine("Failed to register meow block.");
		
		
		CommandHandler commandHandler = bridge.GetCommandHandler();
		CommandHandler clientComm = bridge.GetClientCommandHandler();

		CommandBuilder testDataCommand = commandHandler.CreateCommand("testdata")
			.AddArgument("name", "string", true, "CUSTOM:player_names")
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
			// completion would be the string that the user has already typed in the chat. (Can be used to filter/sort)
			List<string> playerNames = await ctx.GetPlayerNames();
			
			return playerNames;
		});

		CommandBuilder commandWithCustomSuggestions = commandHandler.CreateCommand("command_with_custom_suggestions")
			.AddArgument("player_name", "string", false, "CUSTOM:player_names")
			.Executes(ExecuteCommandWithCustomSuggestions);

		await commandHandler.RegisterCommandAsync(commandWithCustomSuggestions);

		CommandBuilder commandWithSuggestions = commandHandler.CreateCommand("command_with_suggestions")
			.AddArgument("entity", "string", false, "SUMMONABLE_ENTITIES")
			.Executes(ExecuteCommandWithSuggestions);

		await commandHandler.RegisterCommandAsync(commandWithSuggestions);


		CommandBuilder testCommand = commandHandler.CreateCommand("test_command")
			.Executes(ExecuteTestCommand);

		await commandHandler.RegisterCommandAsync(testCommand);

		CommandBuilder requiredCommand = commandHandler.CreateCommand("required_command")
			.Requires(async source => await source.HasPermissionLevel(1))
			.Executes(ExecuteRequiredCommand);

		await commandHandler.RegisterCommandAsync(requiredCommand);


		CommandBuilder subCommandOne = commandHandler.CreateCommand("sub_command_one")
			.Executes(ExecuteSubCommandOne);

		CommandBuilder commandOne = commandHandler.CreateCommand("command_one")
			.AddSubCommand(subCommandOne);
		
		await commandHandler.RegisterCommandAsync(commandOne);


		CommandBuilder subCommandTwo = commandHandler.CreateCommand("sub_command_two")
			.Executes(ExecuteSubCommandTwo);

		CommandBuilder commandTwo = commandHandler.CreateCommand("command_two")
			.Executes(ExecuteCommandTwo)
			.AddSubCommand(subCommandTwo);
		
		await commandHandler.RegisterCommandAsync(commandTwo);

		

		CommandBuilder commandWithArg = commandHandler.CreateCommand("command_with_arg")
			.Executes(ExecuteCommandWithArg)
			.AddArgument("value", "integer");

		await commandHandler.RegisterCommandAsync(commandWithArg);

		CommandBuilder commandWithTwoArgs = commandHandler.CreateCommand("command_with_two_args")
			.Executes(ExecuteCommandWithMultipleArgs)
			.AddArgument("value_one", "integer")
			.AddArgument("value_two", "integer", optional: true);

		await commandHandler.RegisterCommandAsync(commandWithTwoArgs);
		
		
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

	private static async Task ExecuteCommandWithCustomSuggestions(CommandContext context)
	{
		string name = context.Payload["player_name"];
		await context.SendFeedback($"Called /command_with_custom_suggestions with value = {name}.");
	}

	private static async Task ExecuteCommandWithSuggestions(CommandContext context)
	{
		string entityType = context.Payload["entity"];
		await context.SendFeedback($"Called /command_with_suggestions with entity {entityType}.");
	}

	private static async Task ExecuteCommandWithMultipleArgs(CommandContext context)
	{
		if (context.Payload.Count == 1)
		{
			// We can safely assume this exists, because it is not optional.
			string valueOne = context.Payload["value_one"];
			await context.SendFeedback($"Called /command_with_two_args with value one = {valueOne}.");
		}
		else
		{
			string valueOne = context.Payload["value_one"];
			string valueTwo = context.Payload["value_two"];
			await context.SendFeedback($"Called /command_with_two_args with value one = {valueOne} and value two = {valueTwo}.");
		}
	}

	private static async Task ExecuteCommandWithArg(CommandContext context)
	{
		// It can be safely assumed that this is a integer, otherwise minecraft wouldn't execute it.
		string value = context.Payload["value"];
		await context.SendFeedback($"Called /command_with_arg with value = {value}.");
	}

	private static async Task ExecuteCommandTwo(CommandContext context)
	{
		await context.SendFeedback("Called /command_two");
	}

	private static async Task ExecuteSubCommandTwo(CommandContext context)
	{
		await context.SendFeedback("Called /sub_command_two");
	}

	private static async Task ExecuteSubCommandOne(CommandContext context)
	{
		await context.SendFeedback("Called /sub_command_one");
	}

	private static async Task ExecuteRequiredCommand(CommandContext context)
	{
		await context.SendFeedback("Called /required_command");
	}

	private static async Task ExecuteTestCommand(CommandContext context)
	{
		await context.SendFeedback("Called /test_command");
	}
}