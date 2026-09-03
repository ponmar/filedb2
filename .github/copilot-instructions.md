# Copilot Instructions

## Git

Never stage, commit, or push code changes. Leave all version control operations to the developer.

## Project Overview

FileDB is a C# desktop application for storing and managing metadata (persons, locations, tags) for a file collection (photos, videos). It uses an SQLite database backed by Dapper and is built with Avalonia UI targeting .NET 10.

## Solution Structure

- **`FileDB/`** – Main Avalonia UI application (MVVM, all UI logic)
- **`FileDB.Desktop/`** – Desktop entry point (startup project); pass a `.FileDB` file as a command-line argument to open a collection
- **`FileDBInterface/`** – Database and filesystem access library; no UI dependencies
- **`FileDB.Tests/`** – xUnit tests for `FileDB` (uses FakeItEasy)
- **`FileDBInterface.Tests/`** – xUnit tests for `FileDBInterface`
- **`MediaFilesHelper2/` + `MediaFilesHelper2.Desktop/`** – Separate helper application, not part of `FileDB.slnx`

## Build & Test Commands

```sh
# Build entire solution
dotnet build

# Run all tests
dotnet test

# Run tests for a specific project
dotnet test FileDB.Tests
dotnet test FileDBInterface.Tests

# Run a single test by name
dotnet test --filter "FullyQualifiedName~BirthdaysViewModelTests"
```

CI runs on the `dev` branch via `.github/workflows/dotnet.yml` (Ubuntu, .NET 10).

## Architecture

### Layers
- **`FileDBInterface`** handles all SQLite access (via Dapper) through `IDatabaseAccess` (composed of `IPersonAccess`, `ILocationAccess`, `ITagAccess`, `IFilesAccess`) and filesystem access via `IFilesystemAccess`.
- **`FileDB`** is the UI layer. It never accesses the database directly — always through the interfaces provided by `FileDBInterface`.

### Dependency Injection
`ServiceLocator` wraps Castle.Windsor. All registrations are done in `Bootstrapper.Bootstrap()`. ViewModels and services are registered as either singletons or transients. To resolve a service: `ServiceLocator.Resolve<T>()`.

### Central State — `Model`
`Model` (`FileDB/Model/Model.cs`) is the singleton that holds the active `Config`, `IDatabaseAccess`, and `IFilesystemAccess`. It implements `IConfigProvider`, `IDatabaseAccessProvider`, `IFilesystemAccessProvider`, and `IConfigUpdater`. Config changes are propagated by sending a `ConfigUpdated` message.

### Messaging
Inter-component communication uses `CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger`, wrapped by the static `Messenger` class. All message types are record types defined in `FileDB/Model/Messages.cs`. Subscribe using the extension method:
```csharp
this.RegisterForEvent<SomeMessage>(msg => { ... });
```

### DatabaseCache
`DatabaseCache` (`FileDB/Model/DatabaseCache.cs`) is a singleton in-memory cache for Persons, Locations, and Tags. It listens for `PersonEdited`/`LocationEdited`/`TagEdited`/`ConfigUpdated` messages and reloads from the database automatically, then sends `PersonsUpdated`/`LocationsUpdated`/`TagsUpdated` to notify UI components.

### MVVM Pattern
ViewModels use `CommunityToolkit.Mvvm` (`ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`). Each Avalonia view (`*.axaml` + `*.axaml.cs`) has a corresponding ViewModel. Search filters each have their own ViewModel, registered as transients in `Bootstrapper`.

## Key Conventions

- **Nullable reference types** are enabled globally via `Directory.Build.props`.
- **`OS_WINDOWS`** preprocessor constant is set on Windows builds; use `#if OS_WINDOWS` for platform-specific code.
- **UI strings** live in `FileDB/Lang/Strings.resx`; access via `Strings.ResourceName` (auto-generated `Strings.Designer.cs`). Add new strings through the .resx file.
- **Mocking in tests** uses FakeItEasy (`A.Fake<T>()`, `A.CallTo(...).Returns(...)`).
- **Branch strategy**: `master` = release tags; `dev` = active development; PRs target `dev`.
- **Config file**: a `.FileDB` file in the file collection root is the application entry point, not a project config file.
- **`Avalonia.Diagnostics`** package is included only in `Debug` builds (see `FileDB.csproj`).
