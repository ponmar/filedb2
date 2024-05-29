using FileDB.Configuration;
using FileDB.Migrators;
using Xunit;

namespace FileDBTests.Migrators;

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
