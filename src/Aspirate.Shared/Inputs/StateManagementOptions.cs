namespace Aspirate.Shared.Inputs;

public class StateManagementOptions
{
    public required AspirateState State { get; set; }
    public required bool? DisableState { get; set; }
    public required bool? NonInteractive { get; set; } = false;
    public required bool? RequiresState { get; set; } = false;
    public required string StatePath { get; set; }
}
