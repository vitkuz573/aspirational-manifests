using Aspirate.Shared.Enums;
using Aspirate.Shared.Extensions;
using Aspirate.Shared.Interfaces.Commands;
using Aspirate.Shared.Interfaces.Commands.Contracts;
using Aspirate.Shared.Models.Aspirate;
using Aspirate.Shared.Models.AspireManifests.Components.Common;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Aspirate.Tests.ExtensionTests;

public class AspirateStateExtensionsTests
{
    private class TestOptions : ICommandOptions, IContainerOptions
    {
        public bool? NonInteractive { get; set; }
        public bool? DisableSecrets { get; set; }
        public bool? DisableState { get; set; }
        public string? SecretPassword { get; set; }
        public string? LaunchProfile { get; set; }
        public string? SecretProvider { get; set; }
        public int? Pbkdf2Iterations { get; set; }
        public string? StatePath { get; set; }

        public string? ContainerBuilder { get; set; }
        public string? ContainerBuildContext { get; set; }
        public string? ContainerRegistry { get; set; }
        public string? ContainerRepositoryPrefix { get; set; }
        public List<string>? ContainerImageTags { get; set; }
        public List<string>? ContainerBuildArgs { get; set; }
        public string? OverlayPath { get; set; }
    }

    [Fact]
    public void PopulateStateFromOptions_SetsDefaultContainerBuilder_WhenNoneProvided()
    {
        // Arrange
        var state = new AspirateState();
        var options = new TestOptions();

        // Act
        state.PopulateStateFromOptions(options);

        // Assert
        state.ContainerBuilder.Should().Be(ContainerBuilder.Docker.Value);
    }

    [Fact]
    public void PopulateStateFromOptions_OverlayPathStored()
    {
        var state = new AspirateState();
        var options = new TestOptions { OverlayPath = "/over" };

        state.PopulateStateFromOptions(options);

        state.OverlayPath.Should().Be("/over");
    }

    [Fact]
    public void GetResourcesWithExternalBindings_ReturnsExpectedResources()
    {
        // Arrange
        var state = new AspirateState
        {
            LoadedAspireManifestResources = new Dictionary<string, Resource>
            {
                ["web"] = new ProjectResource
                {
                    Bindings = new Dictionary<string, Binding>
                    {
                        ["http"] = new Binding
                        {
                            Scheme = "http",
                            Protocol = "tcp",
                            Transport = "http",
                            External = true
                        }
                    }
                },
                ["internal"] = new ProjectResource
                {
                    Bindings = new Dictionary<string, Binding>
                    {
                        ["http"] = new Binding
                        {
                            Scheme = "http",
                            Protocol = "tcp",
                            Transport = "http",
                            External = false
                        }
                    }
                }
            }
        };

        // Act
        var results = state.GetResourcesWithExternalBindings().Select(r => r.Key).ToList();

        // Assert
        results.Should().ContainSingle().Which.Should().Be("web");
    }
}
