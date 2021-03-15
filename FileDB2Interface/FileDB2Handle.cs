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

        #region Files

        public List<FilesModel> GetFiles()
        {
            using var connection = CreateConnection();
            return connection.Query<FilesModel>("select * from files", new DynamicParameters()).ToList();
        }

        public List<FilesModel> SearchFiles(string criteria)
        {
            using var connection = CreateConnection();
            var sql = "select * from files where (path like @criteria or description like @criteria)";
            return connection.Query<FilesModel>(sql, new { criteria = "%" + criteria + "%" }).ToList();
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

        #endregion

        #region Persons

        public List<PersonModel> GetPersons()
        {
            using var connection = CreateConnection();
            return connection.Query<PersonModel>("select * from persons", new DynamicParameters()).ToList();
        }

        public List<PersonModel> SearchPersons(string criteria)
        {
            using var connection = CreateConnection();
            var sql = "select * from persons where (firstname like @criteria or lastname like @criteria or description like @criteria)";
            return connection.Query<PersonModel>(sql, new { criteria = "%" + criteria + "%" }).ToList();
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

        #endregion

        #region Locations

        public List<LocationModel> GetLocations()
        {
            using var connection = CreateConnection();
            var output = connection.Query<LocationModel>("select * from locations", new DynamicParameters());
            return output.ToList();
        }

        public List<LocationModel> SearchLocations(string criteria)
        {
            using var connection = CreateConnection();
            var sql = "select * from locations where (name like @criteria or description like @criteria)";
            return connection.Query<LocationModel>(sql, new { criteria = "%" + criteria + "%" }).ToList();
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

        #endregion

        #region Tags

        public List<TagModel> GetTags()
        {
            using var connection = CreateConnection();
            var output = connection.Query<TagModel>("select * from tags", new DynamicParameters());
            return output.ToList();
        }

        public List<TagModel> SearchTags(string criteria)
        {
            using var connection = CreateConnection();
            var sql = "select * from tags where (name like @criteria)";
            return connection.Query<TagModel>(sql, new { criteria = "%" + criteria + "%" }).ToList();
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

        #endregion

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
