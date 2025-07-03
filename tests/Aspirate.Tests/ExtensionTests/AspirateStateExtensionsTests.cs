using Aspirate.Shared.Enums;
using Aspirate.Shared.Extensions;
using Aspirate.Shared.Interfaces.Commands;
using Aspirate.Shared.Interfaces.Commands.Contracts;
using Aspirate.Shared.Models.Aspirate;
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
}
