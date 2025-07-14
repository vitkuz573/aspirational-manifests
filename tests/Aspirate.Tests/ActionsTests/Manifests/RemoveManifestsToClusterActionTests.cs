using System;
using System.Threading.Tasks;
using Xunit;

namespace Aspirate.Tests.ActionsTests.Manifests;

public class RemoveManifestsToClusterActionTests : BaseActionTests<RemoveManifestsFromClusterAction>
{
    private const string ContextsResponse =
        """
        {
          "contexts": [
            {
              "name": "docker-desktop",
              "context": {
                "cluster": "docker-desktop",
                "user": "docker-desktop"
              }
            },
            {
              "name": "experiments",
              "context": {
                "cluster": "experiments",
                "user": "experiments"
              }
            }
          ],
          "current-context": "docker-desktop"
        }
        """;

    [Fact]
    public async Task ExecuteRemoveManifestsToClusterActionTests_Interactive_Success()
    {
        // Arrange
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Input.PushTextWithEnter("y");
        console.Input.PushKey(ConsoleKey.Enter);
        var state = CreateAspirateState(projectPath: DefaultProjectPath, inputPath: "/some-path");
        var serviceProvider = CreateServiceProvider(state, console, new FileSystem());

        var mockExecutorService = serviceProvider.GetRequiredService<IShellExecutionService>();

        mockExecutorService.ClearSubstitute();

        mockExecutorService.ExecuteCommand(Arg.Is<ShellCommandOptions>(options => options.Command != null && options.ArgumentsBuilder != null))
            .ReturnsForAnyArgs(new ShellCommandResult(true, ContextsResponse, string.Empty, 0));

        var generateAspireManifestAction = GetSystemUnderTest(serviceProvider);

        // Act
        var result = await generateAspireManifestAction.ExecuteAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateRemoveManifestsToClusterActionTests_NonInteractiveNoContext_ThrowsActionCausesExitException()
    {
        // Arrange
        var state = CreateAspirateState(nonInteractive: true, projectPath: DefaultProjectPath, inputPath: "/some-path");
        var serviceProvider = CreateServiceProvider(state, fileSystem: new FileSystem());
        var generateAspireManifestAction = GetSystemUnderTest(serviceProvider);

        // Act
        var act = () => generateAspireManifestAction.ValidateNonInteractiveState();

        // Assert
        act.Should().Throw<ActionCausesExitException>();
    }

    [Fact]
    public void ValidateRemoveManifestsToClusterActionTests_NonInteractiveContextSet_DoesNotThrow()
    {
        // Arrange
        var state = CreateAspirateState(nonInteractive: true, projectPath: DefaultProjectPath, inputPath: "/some-path", kubeContext: "docker-desktop");
        var serviceProvider = CreateServiceProvider(state, fileSystem: new FileSystem());
        var generateAspireManifestAction = GetSystemUnderTest(serviceProvider);

        // Act
        var act = () => generateAspireManifestAction.ValidateNonInteractiveState();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task ExecuteRemoveManifestsToClusterActionTests_NonInteractive_Success()
    {
        // Arrange
        var state = CreateAspirateState(nonInteractive: true, projectPath: DefaultProjectPath, inputPath: "/some-path", kubeContext: "docker-desktop");
        var serviceProvider = CreateServiceProvider(state, fileSystem: new FileSystem());

        var mockExecutorService = serviceProvider.GetRequiredService<IShellExecutionService>();

        mockExecutorService.ClearSubstitute();

        mockExecutorService.ExecuteCommand(Arg.Is<ShellCommandOptions>(options => options.Command != null && options.ArgumentsBuilder != null))
            .ReturnsForAnyArgs(new ShellCommandResult(true, ContextsResponse, string.Empty, 0));

        var generateAspireManifestAction = GetSystemUnderTest(serviceProvider);

        // Act
        var result = await generateAspireManifestAction.ExecuteAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveManifests_UsesOverlayPath_WhenSet()
    {
        var state = CreateAspirateState(nonInteractive: true, projectPath: DefaultProjectPath, inputPath: "/some-path", kubeContext: "docker-desktop");
        state.OverlayPath = "/overlay";

        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory("/overlay");

        var kubeCtl = Substitute.For<IKubeCtlService>();
        kubeCtl.RemoveManifests(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(true));

        var services = new ServiceCollection();
        services.RegisterAspirateEssential();
        services.RemoveAll<IFileSystem>();
        services.RemoveAll<IAnsiConsole>();
        services.RemoveAll<AspirateState>();
        services.RemoveAll<IKubeCtlService>();

        services.AddSingleton<IFileSystem>(fileSystem);
        services.AddSingleton<IAnsiConsole>(new TestConsole());
        services.AddSingleton(state);
        services.AddSingleton(Substitute.For<IShellExecutionService>());
        services.AddSingleton(kubeCtl);

        var provider = services.BuildServiceProvider();
        var action = GetSystemUnderTest(provider);

        await action.ExecuteAsync();

        await kubeCtl.Received().RemoveManifests(state.KubeContext, "/overlay");
    }
}
