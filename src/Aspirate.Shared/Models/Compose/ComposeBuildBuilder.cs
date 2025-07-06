using DockerComposeBuilder.Builders.Services;
using DockerComposeBuilder.Model.Services;

namespace Aspirate.Shared.Models.Compose;

public class ComposeBuildBuilder : BuildBuilder
{
    private Dictionary<string, ComposeBuildSecret>? _secrets;

    public ComposeBuildBuilder WithSecrets(Dictionary<string, ComposeBuildSecret> secrets)
    {
        _secrets ??= new();
        foreach (var kvp in secrets)
        {
            _secrets[kvp.Key] = kvp.Value;
        }
        return this;
    }

    public ComposeBuildBuilder WithSecrets(Action<Dictionary<string, ComposeBuildSecret>> secretExpression)
    {
        _secrets ??= new();
        secretExpression(_secrets);
        return this;
    }

    public override ServiceBuild Build()
    {
        var baseBuild = base.Build();
        var build = new ComposeBuild
        {
            Context = baseBuild.Context,
            Dockerfile = baseBuild.Dockerfile,
            Arguments = baseBuild.Arguments,
            Secrets = _secrets
        };
        return build;
    }
}
