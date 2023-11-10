using FileDB;
using FileDB.Configuration;
using FileDB.Migrators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileDBTests.Migrators;

[TestClass]
public class ConfigMigratorTests
{
    private ConfigMigrator migrator = new();

    [TestInitialize]
    public void Initialize()
    {
        Bootstrapper.Reset();

        migrator = new();
    }

    [TestMethod]
    public void Migrate()
    {
        var configWithDefaultValues = new ConfigBuilder().Build();

        var migratedConfig = migrator.Migrate(configWithDefaultValues, DefaultConfigs.Default);

        Assert.AreEqual(14, migratedConfig.OverlayTextSize);
        Assert.AreEqual(24, migratedConfig.OverlayTextSizeLarge);
    }
}
