using Profero.Tern.Migrate;
using Profero.Tern.Provider;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Profero.Tern.SqlServer.SystemTests.Bindings
{
    public class TestDataContext : IDisposable
    {
        readonly IDbConnection connection;

        public TestDataContext()
        {
            Versions = new List<Migrate.MigrationVersion>();
            Options = new Migrate.ScriptGenerationOptions();

            AppDomain.CurrentDomain.SetData("DataDirectory",
                Path.GetTempPath());

            this.connection = CreateConnection();
        }

        public IDatabaseScriptGenerator DatabaseProvider { get { return new SqlServerScriptGenerator(); } }

        public List<Tern.Migrate.MigrationVersion> Versions { get; set; }

        public Migrate.ScriptGenerationOptions Options { get; set; }

        public System.Data.IDataReader Execute(string p)
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            var command = connection.CreateCommand();

            command.CommandText = p;
            return command.ExecuteReader();
        }

        public IVersionStrategy VersionStrategy
        {
            get { return new VersionVersionStrategy(); }
        }

        public int ExecuteNonQuery(string p)
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            var command = connection.CreateCommand();

            command.CommandText = p;
            return command.ExecuteNonQuery();
        }

        public void CreateDatabase()
        {
            ExecuteNonQuery("USE master; DROP DATABASE TernTests; CREATE DATABASE TernTests; USE TernTests");
        }

        private IDbConnection CreateConnection()
        {
            ConnectionStringSettings connectionString = ConfigurationManager.ConnectionStrings["Tern.TestDb"];

            var provider = DbProviderFactories.GetFactory(connectionString.ProviderName);

            var connection = provider.CreateConnection();

            connection.ConnectionString = connectionString.ConnectionString;

            return connection;
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        public void SortVersions()
        {
            Versions = MigrationVersion.Sort(Versions, new VersionVersionStrategy()).ToList();
        }
    }
}
