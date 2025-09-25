using Terminal.Gui.Xaml.Build;
using Terminal.Gui.Xaml.Documentation.Services;
using Terminal.Gui.Xaml.Documentation.Services.Implementations;
using Terminal.Gui.Xaml.Documentation.Models;
using Xunit;

namespace Terminal.Gui.Xaml.Tests.Unit;

public class BuildIntegrationServiceTests
{
    [Fact]
    public async Task ExecuteBuildTarget_GenerateDocumentation_Succeeds()
    {
        var simple = new SimpleBuildIntegrationService();
        var wrapper = new BuildIntegrationService(simple);
        var req = new BuildTargetRequest
        {
            Target = "GenerateDocumentation",
            ProjectFile = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csproj", SearchOption.AllDirectories).FirstOrDefault() ?? "src/Terminal.Gui.Xaml/Terminal.Gui.Xaml.csproj",
            Properties = new Dictionary<string,string>{{"OutputPath", Path.Combine(Path.GetTempPath(), "tgxaml-gen-"+Guid.NewGuid().ToString("n"))}}
        };
        var progressEvents = new List<BuildProgress>();
        var resp = await wrapper.ExecuteBuildTargetAsync(req, p => { progressEvents.Add(p); return Task.CompletedTask; });
        Assert.True(resp.Success);
        Assert.Equal(0, resp.ExitCode);
        Assert.Contains(progressEvents, p => p.Phase == BuildPhase.Completed);
        // Ensure ordering progression without regression: Starting -> (Preparing|Building|Validating|Completing) -> Completed
        var phases = progressEvents.Select(p => p.Phase).ToList();
        Assert.True(phases.First() == BuildPhase.Starting, "First phase should be Starting");
        Assert.Equal(BuildPhase.Completed, phases.Last());
        // No phase after Completed
        var completedIndex = phases.LastIndexOf(BuildPhase.Completed);
        Assert.True(completedIndex == phases.Count - 1);
        // Ensure phases are non-decreasing in a defined order map
        var order = new Dictionary<BuildPhase,int>{
            {BuildPhase.Starting,0},{BuildPhase.Preparing,1},{BuildPhase.Building,2},{BuildPhase.Validating,3},{BuildPhase.Completing,4},{BuildPhase.Completed,5},{BuildPhase.Failed,6}
        };
        for (int i=1;i<phases.Count;i++)
        {
            Assert.True(order[phases[i]] >= order[phases[i-1]], $"Phase order regression at index {i}: {phases[i-1]} -> {phases[i]}");
        }
    }

    [Fact]
    public async Task ExecuteBuildTarget_Unknown_Target_Fails()
    {
        var wrapper = new BuildIntegrationService(new SimpleBuildIntegrationService());
        var req = new BuildTargetRequest
        {
            Target = "Nope",
            ProjectFile = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csproj", SearchOption.AllDirectories).FirstOrDefault() ?? "src/Terminal.Gui.Xaml/Terminal.Gui.Xaml.csproj"
        };
        var resp = await wrapper.ExecuteBuildTargetAsync(req);
        Assert.False(resp.Success);
        Assert.Equal(2, resp.ExitCode);
    }

    [Fact]
    public async Task ExecuteBuildTarget_ValidateDocumentation_Returns_ValidationResults()
    {
        var wrapper = new BuildIntegrationService(new SimpleBuildIntegrationService());
        var req = new BuildTargetRequest
        {
            Target = "ValidateDocumentation",
            ProjectFile = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csproj", SearchOption.AllDirectories).FirstOrDefault() ?? "src/Terminal.Gui.Xaml/Terminal.Gui.Xaml.csproj"
        };
        var resp = await wrapper.ExecuteBuildTargetAsync(req);
        Assert.True(resp.Success);
        Assert.NotNull(resp.ValidationResults);
        Assert.True(resp.ValidationResults!.PassesRequirements);
    }

    [Fact]
    public async Task ExecuteBuildTarget_CleanDocumentation_Deletes_Output()
    {
        var wrapper = new BuildIntegrationService(new SimpleBuildIntegrationService());
        var tempDir = Path.Combine(Path.GetTempPath(), "tgxaml-clean-"+Guid.NewGuid().ToString("n"));
        var req = new BuildTargetRequest
        {
            Target = "CleanDocumentation",
            ProjectFile = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csproj", SearchOption.AllDirectories).FirstOrDefault() ?? "src/Terminal.Gui.Xaml/Terminal.Gui.Xaml.csproj",
            Properties = new Dictionary<string,string>{{"OutputPath", tempDir}}
        };
        var resp = await wrapper.ExecuteBuildTargetAsync(req);
        Assert.True(resp.Success);
        Assert.Equal("Cleaned", resp.Output);
    }
}
