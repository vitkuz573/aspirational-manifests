using System;
using System.Threading.Tasks;
using Aspirate.Cli;
using Xunit;

namespace Aspirate.Tests.ActionsTests.Manifests;

public class ApplyManifestsToClusterActionTests : BaseActionTests<ApplyManifestsToClusterAction>
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
    public async Task ExecuteApplyManifestsToClusterActionTests_Interactive_Success()
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
    public void ValidateApplyManifestsToClusterActionTests_NonInteractiveNoContext_ThrowsActionCausesExitException()
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
    public void ValidateApplyManifestsToClusterActionTests_NonInteractiveContextSet_DoesNotThrow()
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
    public async Task ExecuteApplyManifestsToClusterActionTests_NonInteractive_Success()
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
    public async Task ExecuteApplyManifestsToClusterActionTests_ImagePullSecretApplied()
    {
        var state = CreateAspirateState(nonInteractive: true, projectPath: DefaultProjectPath, inputPath: "/some-path", kubeContext: "docker-desktop");
        state.WithPrivateRegistry = true;

        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(fileSystem.Path.GetTempPath());

        var kubeCtl = Substitute.For<IKubeCtlService>();
        kubeCtl.ApplyManifests(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(true));
        kubeCtl.ApplyManifestFile(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(true));

        var kustomize = Substitute.For<IKustomizeService>();
        kustomize.WriteImagePullSecretToTempFile(state, Arg.Any<ISecretProvider>())
            .Returns(Task.FromResult<string?>("temp.yaml"));

        var secretProvider = new SecretProvider(fileSystem);

        var services = new ServiceCollection();
        services.RegisterAspirateEssential();
        services.RemoveAll<IFileSystem>();
        services.RemoveAll<IShellExecutionService>();
        services.RemoveAll<IAnsiConsole>();
        services.RemoveAll<AspirateState>();
        services.RemoveAll<IKubeCtlService>();
        services.RemoveAll<IKustomizeService>();

        services.AddSingleton<IFileSystem>(fileSystem);
        services.AddSingleton<ISecretProvider>(secretProvider);
        services.AddSingleton<IAnsiConsole>(new TestConsole());
        services.AddSingleton(state);
        services.AddSingleton(Substitute.For<IShellExecutionService>());
        services.AddSingleton(kubeCtl);
        services.AddSingleton(kustomize);

        var provider = services.BuildServiceProvider();

        var action = GetSystemUnderTest(provider);

        await action.ExecuteAsync();

        await kubeCtl.Received().ApplyManifestFile(state.KubeContext, "temp.yaml");
        kustomize.Received().CleanupSecretEnvFiles(state.DisableSecrets, Arg.Is<IEnumerable<string>>(x => x.Contains("temp.yaml")));
    }

    [Fact]
    public async Task ExecuteApplyManifestsToClusterActionTests_WithOverlayInput_Success()
    {
        // Arrange
        var overlayPath = "/overlay";
        var outputPath = "/overlay/aspirate-output";

        var state = CreateAspirateState(nonInteractive: true, projectPath: DefaultProjectPath, inputPath: overlayPath, kubeContext: "docker-desktop");
        state.OutputPath = outputPath;

        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(overlayPath);

        var kubeCtl = Substitute.For<IKubeCtlService>();
        kubeCtl.ApplyManifests(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(true));

        var kustomize = Substitute.For<IKustomizeService>();

        var secretProvider = new SecretProvider(fileSystem);

        var services = new ServiceCollection();
        services.RegisterAspirateEssential();
        services.RemoveAll<IFileSystem>();
        services.RemoveAll<IShellExecutionService>();
        services.RemoveAll<AspirateState>();
        services.RemoveAll<IKubeCtlService>();
        services.RemoveAll<IKustomizeService>();
        services.RemoveAll<SecretProvider>();

        services.AddSingleton<IFileSystem>(fileSystem);
        services.AddSingleton(secretProvider);
        services.AddSingleton<IAnsiConsole>(new TestConsole());
        services.AddSingleton(state);
        services.AddSingleton(Substitute.For<IShellExecutionService>());
        services.AddSingleton(kubeCtl);
        services.AddSingleton(kustomize);

        var provider = services.BuildServiceProvider();

        var action = GetSystemUnderTest(provider);

        // Act
        var result = await action.ExecuteAsync();

        // Assert
        result.Should().BeTrue();
        await kubeCtl.Received().ApplyManifests(state.KubeContext!, overlayPath);
    }
}
