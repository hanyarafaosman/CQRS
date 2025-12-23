# NuGet Package Creation Guide

## Building the Package

To create a NuGet package from the CQRS.Mediator library:

```bash
cd src\CQRS.Mediator
dotnet pack -c Release
```

The package will be created in: `src\CQRS.Mediator\bin\Release\CQRS.Mediator.1.0.0.nupkg`

## Publishing to NuGet.org

1. **Get an API Key**
   - Go to https://www.nuget.org/
   - Sign in or create an account
   - Go to your account settings
   - Create a new API key

2. **Push the package**
   ```bash
   dotnet nuget push src\CQRS.Mediator\bin\Release\CQRS.Mediator.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
   ```

## Publishing to a Local or Private Feed

```bash
# Add your private feed (one time)
dotnet nuget add source https://your-private-feed-url -n MyPrivateFeed -u username -p password

# Push to private feed
dotnet nuget push src\CQRS.Mediator\bin\Release\CQRS.Mediator.1.0.0.nupkg --source MyPrivateFeed
```

## Testing the Package Locally

Before publishing, you can test the package locally:

1. **Create a local NuGet source**
   ```bash
   mkdir C:\LocalNuGet
   dotnet nuget add source C:\LocalNuGet -n LocalFeed
   ```

2. **Copy the package to the local source**
   ```bash
   copy src\CQRS.Mediator\bin\Release\*.nupkg C:\LocalNuGet\
   ```

3. **Install in a test project**
   ```bash
   dotnet add package CQRS.Mediator --source LocalFeed
   ```

## Updating Package Version

Edit `src\CQRS.Mediator\CQRS.Mediator.csproj` and update the version number:

```xml
<Version>1.0.1</Version>
```

Then rebuild the package.

## Package Contents

The NuGet package includes:
- ✅ Compiled DLL (CQRS.Mediator.dll)
- ✅ XML documentation (CQRS.Mediator.xml)
- ✅ All public interfaces and classes
- ✅ Dependency on Microsoft.Extensions.DependencyInjection.Abstractions

## Installation Instructions for Users

Users can install your package using:

```bash
dotnet add package CQRS.Mediator
```

Or via Package Manager Console:
```powershell
Install-Package CQRS.Mediator
```
