using FileDBInterface.Model;
using System;
using Dapper;
using Xunit;
using FileDBInterface.DatabaseAccess.SQLite;

namespace FileDBInterfaceTests.DatabaseAccess.SQLite;

public class SqLiteDatabaseCreatorTests
{
    [Fact]
    public void CreateInMemory_ReturnsValidConnection()
    {
        using var connection = new SqLiteDatabaseCreator().CreateInMemory();

        Assert.NotNull(connection);
        Assert.Equal(System.Data.ConnectionState.Open, connection.State);

        var version = connection.ExecuteScalar<int>("pragma user_version;");
        Assert.Equal(2, version);

        connection.Execute("SELECT 1 FROM files");
    }

    [Fact]
    public void CreateInMemory_IncludesBBoxColumnsInSchema()
    {
        using var connection = new SqLiteDatabaseCreator().CreateInMemory();

        try
        {
            connection.Execute("INSERT INTO files (Path) VALUES ('test.jpg')");
            connection.Execute("INSERT INTO persons (ShortName, FullName, Sex) VALUES ('John', 'John Doe', 1)");
            
            var bbox = new PersonBoundingBox(0.1, 0.2, 0.3, 0.4);
            connection.Execute(
                "INSERT INTO filepersons (FileId, PersonId, BBoxX, BBoxY, BBoxWidth, BBoxHeight) VALUES (1, 1, @X, @Y, @Width, @Height)",
                new { bbox.X, bbox.Y, bbox.Width, bbox.Height }
            );
            
            Assert.True(true);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Failed to insert bbox: {ex.Message}");
        }
    }

    [Fact]
    public void CreateInMemory_CanInsertAndRetrieveBoundingBox()
    {
        using var connection = new SqLiteDatabaseCreator().CreateInMemory();

        connection.Execute("INSERT INTO files (Path) VALUES ('test.jpg')");
        connection.Execute("INSERT INTO persons (ShortName, FullName, Sex) VALUES ('John', 'John Doe', 1)");
        
        var bbox = new PersonBoundingBox(0.1, 0.2, 0.3, 0.4);
        connection.Execute(
            "INSERT INTO filepersons (FileId, PersonId, BBoxX, BBoxY, BBoxWidth, BBoxHeight) VALUES (1, 1, @X, @Y, @Width, @Height)",
            new { bbox.X, bbox.Y, bbox.Width, bbox.Height }
        );

        var result = connection.QuerySingle<dynamic>(
            "SELECT BBoxX, BBoxY, BBoxWidth, BBoxHeight FROM filepersons WHERE FileId = 1 AND PersonId = 1"
        );

        Assert.Equal(0.1, (double)result.BBoxX);
        Assert.Equal(0.2, (double)result.BBoxY);
        Assert.Equal(0.3, (double)result.BBoxWidth);
        Assert.Equal(0.4, (double)result.BBoxHeight);
    }
}
