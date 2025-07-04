namespace Aspirate.Shared.Interfaces.Services;

public interface IKubernetesIngressService
{
    Task EnsureIngressController(string context);
}
