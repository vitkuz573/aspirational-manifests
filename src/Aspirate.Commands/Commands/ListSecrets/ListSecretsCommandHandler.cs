namespace Aspirate.Commands.Commands.ListSecrets;

public sealed class ListSecretsCommandHandler(IServiceProvider serviceProvider) : BaseCommandOptionsHandler<ListSecretsOptions>(serviceProvider)
{
    public override Task<int> HandleAsync(ListSecretsOptions options)
    {
        var console = Services.GetRequiredService<IAnsiConsole>();
        var secretProvider = Services.GetRequiredService<ISecretProvider>();

        if (secretProvider.State?.Secrets == null || secretProvider.State.Secrets.Count == 0)
        {
            console.MarkupLine("[yellow]No secrets found.[/]");
            return Task.FromResult(0);
        }

        var resources = secretProvider.State.Secrets.AsEnumerable();

        if (!string.IsNullOrEmpty(options.Resource))
        {
            resources = resources.Where(r => r.Key.Equals(options.Resource, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(options.Provider))
        {
            resources = resources.Where(r =>
                CurrentState.LoadedAspireManifestResources.TryGetValue(r.Key, out var res) &&
                res.Type.Equals(options.Provider, StringComparison.OrdinalIgnoreCase));
        }

        if (!resources.Any())
        {
            console.MarkupLine("[yellow]No matching secrets found.[/]");
            return Task.FromResult(0);
        }

        foreach (var resource in resources)
        {
            console.MarkupLine($"[underline]{resource.Key}[/]");

            var table = new Table()
                .AddColumn("Key")
                .AddColumn("Value");

            foreach (var secret in resource.Value)
            {
                var value = secretProvider.GetSecret(resource.Key, secret.Key);
                var masked = new MaskedValue(value);
                table.AddRow(secret.Key, masked.ToString());
            }

            console.Render(table);
        }

        return Task.FromResult(0);
    }
}
