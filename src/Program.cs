using System.Diagnostics;
using System.Xml.Linq;
using Microsoft.VisualStudio.SolutionPersistence;
using Microsoft.VisualStudio.SolutionPersistence.Model;
using Microsoft.VisualStudio.SolutionPersistence.Serializer;

return await RunAsync(args);

static async Task<int> RunAsync(string[] args)
{
    string? solutionPath = null;
    IEnumerable<string> dotnetRunArguments = args;
    if (args.FirstOrDefault() is string firstArgument &&
        Path.GetExtension(firstArgument).Equals(".slnx", StringComparison.OrdinalIgnoreCase))
    {
        solutionPath = firstArgument;
        dotnetRunArguments = args.Skip(1);
    }

    if (solutionPath is null)
    {
        string[] solutionFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.slnx");
        if (solutionFiles.Length != 1)
        {
            Console.Error.WriteLine("Specify a .slnx file when the current directory does not contain exactly one.");
            return 1;
        }

        solutionPath = solutionFiles[0];
    }

    if (!Path.GetExtension(solutionPath).Equals(".slnx", StringComparison.OrdinalIgnoreCase))
    {
        Console.Error.WriteLine("run-default only supports .slnx solution files.");
        return 1;
    }

    string fullSolutionPath = Path.GetFullPath(solutionPath);
    string? projectPath;
    try
    {
        projectPath = await SlnxStartupProjectResolver.FindAsync(fullSolutionPath);
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Xml.XmlException or SolutionException or InvalidOperationException)
    {
        Console.Error.WriteLine(exception.Message);
        return 1;
    }

    if (projectPath is null)
    {
        Console.Error.WriteLine(SlnxStartupProjectResolver.MissingDefaultStartupMessage);
        return 1;
    }

    ProcessStartInfo startInfo = new("dotnet")
    {
        UseShellExecute = false,
    };
    startInfo.ArgumentList.Add("run");
    startInfo.ArgumentList.Add("--project");
    startInfo.ArgumentList.Add(projectPath);
    foreach (string argument in dotnetRunArguments)
    {
        startInfo.ArgumentList.Add(argument);
    }

    using Process process = Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start dotnet.");
    await process.WaitForExitAsync();
    return process.ExitCode;
}

public static class SlnxStartupProjectResolver
{
    public const string MissingDefaultStartupMessage =
        "Couldn't find a default project to run.  Ensure there is a project marked \"DefaultStartup\" in the .slnx file.";

    public static async Task<string?> FindAsync(string solutionPath, CancellationToken cancellationToken = default)
    {
        await using FileStream stream = File.OpenRead(solutionPath);
        SolutionModel solution = await SolutionSerializers.SlnXml.OpenAsync(stream, cancellationToken);

        await using FileStream xmlStream = File.OpenRead(solutionPath);
        XDocument document = await XDocument.LoadAsync(
            xmlStream,
            LoadOptions.None,
            cancellationToken);

        string? defaultStartupPath = document
            .Descendants()
            .Where(element => element.Name.LocalName == "Project")
            .Where(element => bool.TryParse((string?)element.Attribute("DefaultStartup"), out bool isDefault) && isDefault)
            .Select(element => (string?)element.Attribute("Path"))
            .FirstOrDefault(path => !string.IsNullOrWhiteSpace(path));

        if (defaultStartupPath is null)
        {
            return null;
        }

        string solutionDirectory = Path.GetDirectoryName(solutionPath)
            ?? throw new InvalidOperationException("The solution path must include a directory.");
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
