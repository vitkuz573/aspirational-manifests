namespace Aspirate.Shared.Interfaces.Commands.Contracts;

public interface IListSecretsOptions
{
    string? Resource { get; set; }
    string? Provider { get; set; }
}
