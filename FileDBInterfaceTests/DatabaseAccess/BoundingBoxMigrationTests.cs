using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using FileDBInterface.Validators;
using System;
using System.Data.SQLite;
using System.IO;
using Dapper;
using Xunit;

namespace FileDBInterfaceTests.DatabaseAccess;

public class BoundingBoxMigrationTests : IDisposable
{
    private string? tempDbPath;

    [Fact]
    public void NewDatabase_IncludesBBoxColumnsInSchema()
    {
        // Arrange
        tempDbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
        DatabaseSetup.CreateDatabase(tempDbPath);

        // Act & Assert: Verify bbox columns are in the schema
        using (var connection = new SQLiteConnection($"Data Source={tempDbPath};foreign keys = true"))
        {
            connection.Open();
            
            var version = connection.ExecuteScalar<int>("pragma user_version;");
            Assert.Equal(2, version);

            // Try to insert with bbox values - if columns don't exist, this will throw
            try
            {
                connection.Execute("INSERT INTO files (Path) VALUES ('test.jpg')");
                connection.Execute("INSERT INTO persons (ShortName, FullName, Sex) VALUES ('John', 'John Doe', 1)");
                
                var bbox = new PersonBoundingBox(0.1, 0.2, 0.3, 0.4);
                connection.Execute(
                    "INSERT INTO filepersons (FileId, PersonId, BBoxX, BBoxY, BBoxWidth, BBoxHeight) VALUES (1, 1, @X, @Y, @Width, @Height)",
                    new { bbox.X, bbox.Y, bbox.Width, bbox.Height }
                );
                
                // If we get here, columns exist
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Failed to insert bbox: {ex.Message}");
            }
        }
    }

    [Fact]
    public void NewDatabase_CanInsertAndRetrieveBoundingBox()
    {
        // Arrange
        tempDbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
        DatabaseSetup.CreateDatabase(tempDbPath);

        // Act & Assert: Verify we can insert and retrieve bbox data
        using (var connection = new SQLiteConnection($"Data Source={tempDbPath};foreign keys = true"))
        {
            connection.Open();

            // Create a test file and person
            connection.Execute("INSERT INTO files (Path) VALUES ('test.jpg')");
            connection.Execute("INSERT INTO persons (ShortName, FullName, Sex) VALUES ('John', 'John Doe', 1)");
            
            // Insert a bounding box
            var bbox = new PersonBoundingBox(0.1, 0.2, 0.3, 0.4);
            connection.Execute(
                "INSERT INTO filepersons (FileId, PersonId, BBoxX, BBoxY, BBoxWidth, BBoxHeight) VALUES (1, 1, @X, @Y, @Width, @Height)",
                new { bbox.X, bbox.Y, bbox.Width, bbox.Height }
            );

            // Verify we can retrieve the bbox
            var result = connection.QuerySingle<dynamic>(
                "SELECT BBoxX, BBoxY, BBoxWidth, BBoxHeight FROM filepersons WHERE FileId = 1 AND PersonId = 1"
            );

            Assert.Equal(0.1, (double)result.BBoxX);
            Assert.Equal(0.2, (double)result.BBoxY);
            Assert.Equal(0.3, (double)result.BBoxWidth);
            Assert.Equal(0.4, (double)result.BBoxHeight);
        }
    }

    [Fact]
    public void BoundingBoxValidator_EnforcesRangeConstraints()
    {
        // Arrange
        var validator = new PersonBoundingBoxValidator();

        // Act & Assert: Valid bbox
        var validBBox = new PersonBoundingBox(0.0, 0.0, 1.0, 1.0);
        var validResult = validator.Validate(validBBox);
        Assert.True(validResult.IsValid);

        // Act & Assert: Invalid bbox (out of range)
        var invalidBBox = new PersonBoundingBox(-0.1, 0.5, 0.5, 0.5);
        var invalidResult = validator.Validate(invalidBBox);
        Assert.False(invalidResult.IsValid);

        // Act & Assert: Invalid bbox (width out of range)
        var invalidBBox2 = new PersonBoundingBox(0.5, 0.5, 1.5, 0.3);
        var invalidResult2 = validator.Validate(invalidBBox2);
        Assert.False(invalidResult2.IsValid);
    }

    public void Dispose()
    {
        if (tempDbPath != null && File.Exists(tempDbPath))
        {
            File.Delete(tempDbPath);
        }
    }
}
