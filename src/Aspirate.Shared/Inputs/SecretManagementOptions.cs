namespace Aspirate.Shared.Inputs;

public class SecretManagementOptions
{
    public required bool? DisableSecrets { get; set; }
    public required bool? NonInteractive { get; set; }
    public required string? SecretPassword { get; set; }
    public bool CommandUnlocksSecrets { get; set; }
    public string? SecretProvider { get; set; }
    public int? Pbkdf2Iterations { get; set; }
    public string? StatePath { get; set; }
    public bool? Force { get; set; }
    public required AspirateState State { get; set; }
}
