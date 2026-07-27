using Microsoft.VisualStudio.SolutionPersistence;
using Microsoft.VisualStudio.SolutionPersistence.Model;

return await RunAsync(args);

static async Task<int> RunAsync(string[] args)
{
    string? specifiedSolutionPath = null;
    IEnumerable<string> dotnetRunArguments = args;
    if (args.FirstOrDefault() is string firstArgument &&
        Path.GetExtension(firstArgument).Equals(".slnx", StringComparison.OrdinalIgnoreCase))
    {
        specifiedSolutionPath = firstArgument;
        dotnetRunArguments = args.Skip(1);
    }

    try
    {
        string solutionPath = SlnxFileResolver.ResolvePath(specifiedSolutionPath, Directory.GetCurrentDirectory());
        string? projectPath = await SlnxStartupProjectResolver.FindAsync(solutionPath);
        if (projectPath is null)
        {
            Console.Error.WriteLine(SlnxStartupProjectResolver.MissingDefaultStartupMessage);
            return 1;
        }

        return await ProcessExecutor.RunDotnetAsync(projectPath, dotnetRunArguments);
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Xml.XmlException or SolutionException or InvalidOperationException or ArgumentException)
    {
        Console.Error.WriteLine(exception.Message);
        return 1;
    }
}
