# run-default

`run-default` runs the project marked `DefaultStartup` in a `.slnx` solution, matching Visual Studio's startup-project behavior.

This will be unecessary once [dotnet/sdk #55139 (`dotnet run` can use the .slnx `DefaultStartup` property)](https://github.com/dotnet/sdk/issues/55139) is implemented.

## Requirements

`run-default` targets .NET 10. Install a compatible .NET SDK or runtime before using it.

## Install from NuGet.org

```powershell
dotnet tool install --global dotnet-run-default
```

Update an existing installation:

```powershell
dotnet tool update --global dotnet-run-default
```

## Install locally

Build the tool package:

```powershell
dotnet pack -c Release
```

In a repository where you want to use it, create a local tool manifest if needed and install from the package output:

```powershell
dotnet new tool-manifest
dotnet tool install --local --add-source <path-to-dotnet-run-default>\src\bin\Release dotnet-run-default
```

## Use

Mark a project as the default startup project in the `.slnx` file:

```xml
<Project Path="src/MyApp/MyApp.csproj" DefaultStartup="true" />
```

From a directory containing exactly one `.slnx` file:

```powershell
dotnet run-default
```

Or pass the solution explicitly:

```powershell
dotnet run-default .\MySolution.slnx
```

Additional arguments are forwarded to `dotnet run`:

```powershell
dotnet run-default .\MySolution.slnx --configuration Release
dotnet run-default -- --my-app-option
```

## Publishing

Pushing a tag in the form `v<version>` publishes that version to NuGet.org through
the `Publish NuGet package` GitHub Actions workflow. Before the first release,
configure NuGet Trusted Publishing for this repository and the GitHub `release`
environment as described in the workflow's comments.
