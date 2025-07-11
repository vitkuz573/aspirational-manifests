namespace Aspirate.Commands.Commands;

[ExcludeFromCodeCoverage]
public class GenericCommand : Command
{
    public GenericCommand(string name, string description)
        : base(name, description) =>
        Action = CommandHandler.Create(InvokeCommand);

    private Task<int> InvokeCommand()
    {
        var services = new ServiceCollection();
        services
            .AddSecretProtectionStrategies()
            .AddAspirateState()
            .AddAspirateServices()
            .AddAspirateActions()
            .AddAspirateProcessors()
            .AddAspirateSecretProvider()
            .AddPlaceholderTransformation()
            .AddSingleton(AnsiConsole.Console);

        return ExecuteCommand(services);
    }

    protected virtual Task<int> ExecuteCommand(IServiceCollection services) => Task.FromResult(0);

    protected static Table CreateHelpTable()
    {
        var table = new Table();
        table.AddColumn("Sub Commands");
        table.AddColumn("Description");

        return table;
    }
}
