using Microsoft.VisualStudio.SolutionPersistence.Model;

public static class SlnxStartupProjectResolver
{
    public const string MissingDefaultStartupMessage =
        "Couldn't find a default project to run.  Ensure there is a project marked \"DefaultStartup\" in the .slnx file.";

    public static async Task<string?> FindAsync(string solutionPath, CancellationToken cancellationToken = default)
    {
        SolutionModel solution = await SlnxFileResolver.OpenSolutionAsync(solutionPath, cancellationToken);
        string? defaultStartupPath = (await SlnxFileResolver.OpenXmlAsync(solutionPath, cancellationToken))
            .Descendants()
            .Where(element => element.Name.LocalName == "Project")
            .Where(element => bool.TryParse((string?)element.Attribute("DefaultStartup"), out bool isDefault) && isDefault)
            .Select(element => (string?)element.Attribute("Path"))
            .FirstOrDefault(path => !string.IsNullOrWhiteSpace(path));

        if (defaultStartupPath is null)
        {
            return null;
        }

        string solutionDirectory = SlnxFileResolver.GetSolutionDirectory(solutionPath);
        string defaultStartupFullPath = Path.GetFullPath(Path.Combine(solutionDirectory, defaultStartupPath));

        SolutionProjectModel? project = solution.SolutionProjects.FirstOrDefault(candidate =>
            string.Equals(
                Path.GetFullPath(Path.Combine(solutionDirectory, candidate.FilePath)),
                defaultStartupFullPath,
                StringComparison.OrdinalIgnoreCase));

        return project is null
            ? throw new InvalidOperationException("The default startup project could not be resolved from the solution.")
            : Path.GetFullPath(Path.Combine(solutionDirectory, project.FilePath));
    }
}
