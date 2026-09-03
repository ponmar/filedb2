using FakeItEasy;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.FilesystemAccess;
using Xunit;
using FileDBModel = FileDB.Model.Model;
using ApplicationFilePaths = FileDB.Model.ApplicationFilePaths;

namespace FileDB.Tests.Model;

public class ModelTests
{
    private readonly IDatabaseAccess _innerDbAccess = A.Fake<IDatabaseAccess>();
    private readonly IFilesystemAccess _filesystemAccess = A.Fake<IFilesystemAccess>();
    private readonly ApplicationFilePaths _filePaths = new("/files", "/config.FileDB", "/db.db");

    private FileDBModel CreateInitializedModel(bool readOnly = false)
    {
        var model = new FileDBModel();
        var config = new ConfigBuilder { ReadOnly = readOnly }.Build();
        model.InitConfig(_filePaths, config, _innerDbAccess, _filesystemAccess);
        return model;
    }

    // ── UpdateConfig ReadOnly toggling ─────────────────────────────────────

    [Fact]
    public void UpdateConfig_ReadOnlyChangedFalseToTrue_WrapsDbAccessInReadOnlyDecorator()
    {
        var model = CreateInitializedModel(readOnly: false);

        model.UpdateConfig(new ConfigBuilder { ReadOnly = true }.Build());

        Assert.IsType<ReadOnlyDatabaseAccess>(model.DbAccess);
    }

    [Fact]
    public void UpdateConfig_ReadOnlyChangedFalseToTrue_InnerIsOriginalDbAccess()
    {
        var model = CreateInitializedModel(readOnly: false);

        model.UpdateConfig(new ConfigBuilder { ReadOnly = true }.Build());

        var decorator = Assert.IsType<ReadOnlyDatabaseAccess>(model.DbAccess);
        Assert.Same(_innerDbAccess, decorator.Inner);
    }

    [Fact]
    public void UpdateConfig_ReadOnlyChangedTrueToFalse_UnwrapsToOriginalDbAccess()
    {
        var model = CreateInitializedModel(readOnly: false);
        model.UpdateConfig(new ConfigBuilder { ReadOnly = true }.Build());

        model.UpdateConfig(new ConfigBuilder { ReadOnly = false }.Build());

        Assert.Same(_innerDbAccess, model.DbAccess);
    }

    [Fact]
    public void UpdateConfig_ReadOnlyUnchangedFalse_LeavesDbAccessUnchanged()
    {
        var model = CreateInitializedModel(readOnly: false);

        model.UpdateConfig(new ConfigBuilder { ReadOnly = false }.Build());

        Assert.Same(_innerDbAccess, model.DbAccess);
    }

    [Fact]
    public void UpdateConfig_ReadOnlyUnchangedTrue_LeavesDbAccessUnchanged()
    {
        var model = CreateInitializedModel(readOnly: false);
        model.UpdateConfig(new ConfigBuilder { ReadOnly = true }.Build());
        var wrappedAccess = model.DbAccess;

        model.UpdateConfig(new ConfigBuilder { ReadOnly = true }.Build());

        Assert.Same(wrappedAccess, model.DbAccess);
    }

    [Fact]
    public void UpdateConfig_ToggledTrueTheFalse_OriginalInnerAccessPreserved()
    {
        var model = CreateInitializedModel(readOnly: false);

        model.UpdateConfig(new ConfigBuilder { ReadOnly = true }.Build());
        model.UpdateConfig(new ConfigBuilder { ReadOnly = false }.Build());

        Assert.Same(_innerDbAccess, model.DbAccess);
        Assert.IsNotType<ReadOnlyDatabaseAccess>(model.DbAccess);
    }

    [Fact]
    public void UpdateConfig_ToggledTrueThenFalseThenTrue_WrapsCorrectly()
    {
        var model = CreateInitializedModel(readOnly: false);

        model.UpdateConfig(new ConfigBuilder { ReadOnly = true }.Build());
        model.UpdateConfig(new ConfigBuilder { ReadOnly = false }.Build());
        model.UpdateConfig(new ConfigBuilder { ReadOnly = true }.Build());

        var decorator = Assert.IsType<ReadOnlyDatabaseAccess>(model.DbAccess);
        Assert.Same(_innerDbAccess, decorator.Inner);
    }

    // ── InitConfig with ReadOnly=true (startup) ────────────────────────────

    [Fact]
    public void InitConfig_ReadOnlyTrue_WrapsDbAccessInReadOnlyDecorator()
    {
        var model = new FileDBModel();
        model.InitConfig(_filePaths, new ConfigBuilder { ReadOnly = true }.Build(), _innerDbAccess, _filesystemAccess);

        Assert.IsType<ReadOnlyDatabaseAccess>(model.DbAccess);
    }

    [Fact]
    public void InitConfig_ReadOnlyTrue_InnerIsOriginalDbAccess()
    {
        var model = new FileDBModel();
        model.InitConfig(_filePaths, new ConfigBuilder { ReadOnly = true }.Build(), _innerDbAccess, _filesystemAccess);

        var decorator = Assert.IsType<ReadOnlyDatabaseAccess>(model.DbAccess);
        Assert.Same(_innerDbAccess, decorator.Inner);
    }

    [Fact]
    public void InitConfig_ReadOnlyFalse_DbAccessIsUnwrapped()
    {
        var model = new FileDBModel();
        model.InitConfig(_filePaths, new ConfigBuilder { ReadOnly = false }.Build(), _innerDbAccess, _filesystemAccess);

        Assert.Same(_innerDbAccess, model.DbAccess);
    }
}
