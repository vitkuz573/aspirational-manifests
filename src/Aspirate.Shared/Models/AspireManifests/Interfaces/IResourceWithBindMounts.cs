using BindMount = Aspirate.Shared.Models.AspireManifests.Components.Common.BindMount;
namespace Aspirate.Shared.Models.AspireManifests.Interfaces;

public interface IResourceWithBindMounts : IResource
{
    List<BindMount>? BindMounts { get; set; }
}

