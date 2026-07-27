using System.Diagnostics;

public static class ProcessExecutor
{
    public static async Task<int> RunDotnetAsync(string projectPath, IEnumerable<string> dotnetRunArguments)
    {
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
}
