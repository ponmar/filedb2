using Xunit;

// Avoid running tests in parallel (required for event-handling from Community Toolkit)
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]
