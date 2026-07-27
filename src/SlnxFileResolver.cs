using System.Xml.Linq;
using Microsoft.VisualStudio.SolutionPersistence.Model;
using Microsoft.VisualStudio.SolutionPersistence.Serializer;

public static class SlnxFileResolver
{
    public static string ResolvePath(string? specifiedPath, string currentDirectory)
    {
        if (specifiedPath is not null)
        {
            EnsureSlnxExtension(specifiedPath);
            return Path.GetFullPath(specifiedPath);
        }

        string[] solutionFiles = Directory.GetFiles(currentDirectory, "*.slnx");
        if (solutionFiles.Length != 1)
        {
            throw new InvalidOperationException("Specify a .slnx file when the current directory does not contain exactly one.");
        }

        return solutionFiles[0];
    }

    public static async Task<SolutionModel> OpenSolutionAsync(string solutionPath, CancellationToken cancellationToken = default)
    {
        await using FileStream stream = File.OpenRead(solutionPath);
        return await SolutionSerializers.SlnXml.OpenAsync(stream, cancellationToken);
    }

    public static async Task<XDocument> OpenXmlAsync(string solutionPath, CancellationToken cancellationToken = default)
    {
        await using FileStream stream = File.OpenRead(solutionPath);
        return await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
    }

    public static string GetSolutionDirectory(string solutionPath) =>
        Path.GetDirectoryName(solutionPath)
        ?? throw new InvalidOperationException("The solution path must include a directory.");

    private static void EnsureSlnxExtension(string solutionPath)
    {
        if (!Path.GetExtension(solutionPath).Equals(".slnx", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("run-default only supports .slnx solution files.", nameof(solutionPath));
        }
    }
}
