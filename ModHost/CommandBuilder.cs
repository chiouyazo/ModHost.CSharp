using ModHost.Models;

namespace ModHost;

public class CommandBuilder
{
    private readonly string _name;
    private readonly List<CommandArgument> _arguments = new List<CommandArgument>();
    private readonly List<CommandBuilder> _subCommands = new List<CommandBuilder>();
    private Func<CommandContext, Task>? _executeCallback;
    private Func<CommandSource, Task<bool>>? _requirementCallback;

    public CommandBuilder(string name)
    {
        _name = name;
    }

    public CommandBuilder AddArgument(string name, string type, bool optional = false, string? suggestionProviderId = null)
    {
        _arguments.Add(new CommandArgument
        {
            Name = name,
            Type = type,
            IsOptional = optional,
            SuggestionProviderId = suggestionProviderId
        });
        return this;
    }

    public CommandBuilder AddArgument(CommandArgument arg)
    {
        _arguments.Add(arg);
        return this;
    }

    public CommandBuilder AddSubCommand(string name, Action<CommandBuilder> configure)
    {
        CommandBuilder sub = new CommandBuilder(name);
        configure(sub);
        _subCommands.Add(sub);
        return this;
    }

    public CommandBuilder AddSubCommand(CommandBuilder builder)
    {
        _subCommands.Add(builder);
        return this;
    }

    public CommandBuilder Executes(Func<CommandContext, Task> executeCallback)
    {
        _executeCallback = executeCallback;
        return this;
    }

    public CommandBuilder Requires(Func<CommandSource, Task<bool>> requirementCallback)
    {
        _requirementCallback = requirementCallback;
        return this;
    }
    
    public string Name => _name;
    public IReadOnlyList<CommandArgument> Arguments => _arguments;
    public IReadOnlyList<CommandBuilder> SubCommands => _subCommands;
    public Func<CommandContext, Task>? ExecuteCallback => _executeCallback;
    public Func<CommandSource, Task<bool>>? RequirementCallback => _requirementCallback;

    // Builds the command string, recursively including subcommands
    public string BuildCommandDefinition()
    {
        string argsPart = string.Join(",", _arguments.Select(arg =>
        {
            List<string> parts = new List<string> { arg.Name + ":" + arg.Type };
            parts.Add(arg.IsOptional ? "optional" : "required");
            
            if (!string.IsNullOrEmpty(arg.SuggestionProviderId))
                parts.Add($"suggestion={arg.SuggestionProviderId}");
            
            return string.Join("|", parts);
        }));

        string subcommandsPart = "";
        if (_subCommands.Count > 0)
        {
            // Recursively build subcommands separated by semicolons
            IEnumerable<string> subDefs = _subCommands.Select(sc => sc.BuildCommandDefinition());
            subcommandsPart = $"|subcommands[{string.Join(";", subDefs)}]";
        }

        if (!string.IsNullOrEmpty(argsPart))
            return $"{_name}|{argsPart}{subcommandsPart}";
        else if (!string.IsNullOrEmpty(subcommandsPart))
            return $"{_name}{subcommandsPart}";
        else
            return _name;
    }
}