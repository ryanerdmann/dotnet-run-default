using Xunit;

public sealed class SlnxStartupProjectResolverTests
{
    [Fact]
    public async Task FindAsync_ReturnsTheDefaultStartupProject()
    {
        string solutionPath = GetSolutionPath("DefaultStartup.slnx");

        string? projectPath = await SlnxStartupProjectResolver.FindAsync(solutionPath);

        Assert.Equal(
            Path.Combine(Path.GetDirectoryName(solutionPath)!, "src", "App.csproj"),
            projectPath);
    }

    [Fact]
    public async Task FindAsync_ReturnsNullWhenNoProjectIsMarkedDefaultStartup()
    {
        string solutionPath = GetSolutionPath("NoDefaultStartup.slnx");

        string? projectPath = await SlnxStartupProjectResolver.FindAsync(solutionPath);

        Assert.Null(projectPath);
    }

    [Fact]
    public async Task FindAsync_FindsAProjectInsideAFolder()
    {
        string solutionPath = GetSolutionPath("NestedDefaultStartup.slnx");

        string? projectPath = await SlnxStartupProjectResolver.FindAsync(solutionPath);

        Assert.Equal(
            Path.Combine(Path.GetDirectoryName(solutionPath)!, "src", "Tools", "Runner.csproj"),
            projectPath);
    }

    private static string GetSolutionPath(string fileName) =>
        Path.Combine(AppContext.BaseDirectory, "TestData", fileName);
}
