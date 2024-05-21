using FileDBAvalonia.Configuration;
using FileDBAvalonia.Migrators;
using Xunit;

namespace FileDBAvaloniaTests.Migrators;

public class ConfigMigratorTests
{
    private ConfigMigrator migrator;

    public ConfigMigratorTests()
    {
        migrator = new();
    }

    [Fact]
    public void Migrate()
    {
        var configWithDefaultValues = new ConfigBuilder().Build();

        var migratedConfig = migrator.Migrate(configWithDefaultValues, DefaultConfigs.Default);

        Assert.Equal(14, migratedConfig.OverlayTextSize);
        Assert.Equal(24, migratedConfig.OverlayTextSizeLarge);
    }
}
