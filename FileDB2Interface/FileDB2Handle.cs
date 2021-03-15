using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text;
using System.Linq;
using Dapper;
using FileDB2Interface.Model;

namespace FileDB2Interface
{
    public class FileDB2Handle
    {
        private readonly FileDB2Config config;

        public FileDB2Handle(FileDB2Config config)
        {
            this.config = config;
        }

        public List<FilesModel> GetFiles()
        {
            using var connection = CreateConnection();
            return connection.Query<FilesModel>("select * from files", new DynamicParameters()).ToList();
        }

        public FilesModel GetFileById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ID", id, DbType.Int32, ParameterDirection.Input);

            using var connection = CreateConnection();
            return connection.QueryFirst<FilesModel>("select * from files where id = @ID", parameters);
        }

        public void DeleteFile(int id)
        {
            using IDbConnection connection = CreateConnection();
            var sql = "delete from files where id = @id";
            connection.Execute(sql, new { id = id });
        }

        public List<PersonModel> GetPersons()
        {
            using var connection = CreateConnection();
            return connection.Query<PersonModel>("select * from persons", new DynamicParameters()).ToList();
        }

        public PersonModel GetPersonById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ID", id, DbType.Int32, ParameterDirection.Input);

            using var connection = CreateConnection();
            return connection.QueryFirst<PersonModel>("select * from persons where id = @ID", parameters);
        }

        public void InsertPerson(string firstname, string lastname, string description = null, string dateOfBirth = null, int profileFileId = 0)
        {
            using var connection = CreateConnection();
            var person = new PersonModel() { firstname = firstname, lastname = lastname, description = description, dateofbirth = dateOfBirth, profilefileid = profileFileId };
            // TODO: other sql without profileFileId if not used (is zero)?
            var sql = "insert into persons (firstname, lastname, description, dateofbirth, profilefileid) values (@firstname, @lastname, @description, @dateofbirth, @profilefileid)";
            connection.Execute(sql, person);
        }

        public void DeletePerson(int id)
        {
            using IDbConnection connection = CreateConnection();
            var sql = "delete from persons where id = @id";
            connection.Execute(sql, new { id = id });
        }

        public List<LocationModel> GetLocations()
        {
            using var connection = CreateConnection();
            var output = connection.Query<LocationModel>("select * from locations", new DynamicParameters());
            return output.ToList();
        }

        public LocationModel GetLocationById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ID", id, DbType.Int32, ParameterDirection.Input);

            using var connection = CreateConnection();
            return connection.QueryFirst<LocationModel>("select * from locations where id = @ID", parameters);
        }

        public void DeleteLocation(int id)
        {
            using IDbConnection connection = CreateConnection();
            var sql = "delete from locations where id = @id";
            connection.Execute(sql, new { id = id });
        }

        public List<TagModel> GetTags()
        {
            using var connection = CreateConnection();
            var output = connection.Query<TagModel>("select * from tags", new DynamicParameters());
            return output.ToList();
        }

        public TagModel GetTagById(int id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ID", id, DbType.Int32, ParameterDirection.Input);

            using var connection = CreateConnection();
            return connection.QueryFirst<TagModel>("select * from tags where id = @ID", parameters);
        }

        public void DeleteTag(int id)
        {
            using IDbConnection connection = CreateConnection();
            var sql = "delete from tags where id = @id";
            connection.Execute(sql, new { id = id });
        }

        private IDbConnection CreateConnection()
        {
            var connectionString = $"Data Source={config.Database};Version=3;";
            return new SQLiteConnection(connectionString);
        }

        // TODO: InsertFile, InsertLocation, InsertTag, ...

        // TODO: delete
        // TODO: update
        // TODO: import files
    }
}
