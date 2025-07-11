namespace Aspirate.Commands.Commands;

[ExcludeFromCodeCoverage]
public abstract class BaseCommand<TOptions, TOptionsHandler> : Command
    where TOptions : BaseCommandOptions
    where TOptionsHandler : class, ICommandOptionsHandler<TOptions>
{
    protected abstract bool CommandUnlocksSecrets { get; }
    protected virtual bool CommandSkipsStateAndSecrets => false;
    protected virtual bool CommandAlwaysRequiresState => false;

    protected BaseCommand(string name, string description) : base(name, description)
    {
        Options.Add(NonInteractiveOption.Instance);
        Options.Add(DisableSecretsOption.Instance);
        Options.Add(DisableStateOption.Instance);
        Options.Add(StatePathOption.Instance);
        Options.Add(LaunchProfileOption.Instance);
        Options.Add(SecretProviderOption.Instance);
        Options.Add(Pbkdf2IterationsOption.Instance);

        Action = CommandHandler.Create<TOptions>(ConstructCommand);
    }

    private async Task<int> ConstructCommand(TOptions options)
    {
        var services = new ServiceCollection();
        services
            .AddSingleton(AnsiConsole.Console)
            .AddSecretProtectionStrategies()
            .AddAspirateState()
            .AddAspirateServices()
            .AddAspirateActions()
            .AddAspirateProcessors()
            .AddAspirateSecretProvider()
            .AddPlaceholderTransformation();

        var handler = ActivatorUtilities.CreateInstance<TOptionsHandler>(services.BuildServiceProvider());

        var versionCheckService = handler.Services.GetRequiredService<IVersionCheckService>();

        await versionCheckService.CheckVersion();

        if (CommandSkipsStateAndSecrets)
        {
            handler.CurrentState.PopulateStateFromOptions(options);

            return await handler.HandleAsync(options);
        }

        var stateService = handler.Services.GetRequiredService<IStateService>();
        var secretService = handler.Services.GetRequiredService<ISecretService>();

        var stateOptions = GetStateManagementOptions(options, handler, CommandAlwaysRequiresState);

        await stateService.RestoreState(stateOptions);

        handler.CurrentState.PopulateStateFromOptions(options);

        await LoadSecrets(options, secretService, handler);

        var exitCode = await handler.HandleAsync(options);

        await stateService.SaveState(stateOptions);

        return exitCode;
    }

    private Task LoadSecrets(TOptions options, ISecretService secretService, TOptionsHandler handler) =>
        secretService.LoadSecretsAsync(new SecretManagementOptions
        {
            DisableSecrets = handler.CurrentState.DisableSecrets,
            NonInteractive = options.NonInteractive,
            SecretPassword = options.SecretPassword,
            SecretProvider = handler.CurrentState.SecretProvider,
            Pbkdf2Iterations = options.Pbkdf2Iterations,
            CommandUnlocksSecrets = CommandUnlocksSecrets,
            State = handler.CurrentState,
        });

    private static StateManagementOptions GetStateManagementOptions(TOptions options, TOptionsHandler handler, bool requiresState) =>
        new()
        {
            NonInteractive = options.NonInteractive,
            DisableState = options.DisableState,
            StatePath = options.StatePath ?? Directory.GetCurrentDirectory(),
            State = handler.CurrentState,
            RequiresState = requiresState,
        };
}
