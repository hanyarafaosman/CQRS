# Waseet.CQRS Rename - Manual Steps Required

## ✅ COMPLETED AUTOMATICALLY

1. ✅ Updated all .csproj files with new package metadata and namespaces
2. ✅ Updated all C# source files (.cs) - changed namespaces from CQRS.Mediator to Waseet.CQRS
3. ✅ Updated solution file (.sln) references
4. ✅ Updated key documentation files (README.md, PROJECT_SUMMARY.md)

## 📋 MANUAL STEPS REQUIRED

### Step 1: Close VS Code / Visual Studio
Close any IDE that has the project open to release file locks.

### Step 2: Rename Project Directories

From Windows Explorer or Command Prompt:

```cmd
cd c:\Users\hosman-c\source\repos\CQRS

rem Rename the library folder
ren "src\CQRS.Mediator" "Waseet.CQRS"

rem Rename the sample folder  
ren "tests\CQRS.Mediator.Sample" "Waseet.CQRS.Sample"
```

### Step 3: Rename Project Files

```cmd
cd c:\Users\hosman-c\source\repos\CQRS

rem Rename library project file
ren "src\Waseet.CQRS\CQRS.Mediator.csproj" "Waseet.CQRS.csproj"

rem Rename sample project file
ren "tests\Waseet.CQRS.Sample\CQRS.Mediator.Sample.csproj" "Waseet.CQRS.Sample.csproj"
```

### Step 4: Rename Solution File (Optional)

```cmd
ren "CQRS.Mediator.sln" "Waseet.CQRS.sln"
```

### Step 5: Update Remaining Documentation Files

The following markdown files may still contain old references. Update them manually or use Find/Replace:

- FEATURES.md (replace "CQRS.Mediator" with "Waseet.CQRS")
- VALIDATION.md (update using statements)
- EVENTS.md (update using statements)
- STREAMING.md (update using statements and references)
- PACKAGE.md (update paths and package names)

### Step 6: Build and Test

```cmd
cd c:\Users\hosman-c\source\repos\CQRS
dotnet build
dotnet run --project tests\Waseet.CQRS.Sample
```

### Step 7: Create New NuGet Package

```cmd
cd src\Waseet.CQRS
dotnet pack -c Release
```

The package will be created at: `src\Waseet.CQRS\bin\Release\Waseet.CQRS.1.0.0.nupkg`

## 🎉 FINAL RESULT

You'll have a complete library renamed to **Waseet.CQRS (وسيط)** with:

- ✅ Arab identity and branding
- ✅ All code using Waseet.CQRS namespaces
- ✅ Updated project files and solution
- ✅ Ready to publish as Waseet.CQRS on NuGet
- ✅ Built to encourage Arabic developers worldwide

**Waseet** (وسيط) means "Mediator" in Arabic - representing the bridge between commands, queries, and their handlers, while celebrating Arab heritage in software development! 🌍

## 📚 Why Waseet?

- **Cultural Pride**: Celebrating Arab identity in tech
- **Global Community**: Built for developers worldwide
- **Modern .NET**: Full featured mediator pattern library
- **Feature Complete**: Validation, Events, Streaming
- **Developer Friendly**: Simple API, comprehensive docs
