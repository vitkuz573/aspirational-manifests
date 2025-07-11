namespace Aspirate.Commands.Options;

public sealed class IncludeDashboardOption : BaseOption<bool?>
{
    private static readonly string[] _aliases = ["--include-dashboard", "--with-dashboard"];

    private IncludeDashboardOption() : base(nameof(IDashboardOptions.IncludeDashboard), _aliases, "ASPIRATE_INCLUDE_DASHBOARD", null)
    {
        Description = "Include the Aspire Dashboard in the generated manifests";
        Arity = ArgumentArity.ZeroOrOne;
        Required = false;
    }

    public static IncludeDashboardOption Instance { get; } = new();

    public override bool IsSecret => false;
}
