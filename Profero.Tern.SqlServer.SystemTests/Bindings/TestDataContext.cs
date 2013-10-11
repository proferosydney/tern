using System.Data.SqlClient;
using System.IO;
using Profero.Tern.Migrate;
using Profero.Tern.Provider;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace Profero.Tern.SqlServer.SystemTests.Bindings
{
    public class TestDataContext : IDisposable
    {
        DbConnection connection;

        public TestDataContext()
        {
            Versions = new List<Migrate.MigrationVersion>();
            Options = new Migrate.ScriptGenerationOptions();

            AppDomain.CurrentDomain.SetData("DataDirectory",
                Environment.CurrentDirectory);
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
            if (connection != null)
            {
                connection.Dispose();
            }

            string mdfPath = Path.Combine(Environment.CurrentDirectory, "TernTests.mdf");

            using (connection = CreateMasterConnection())
            {
                try
                {
                    ExecuteNonQuery(
                        @"IF db_id('TernTests') is not null BEGIN ALTER DATABASE [TernTests] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [TernTests]; END");
                }
                catch (SqlException)
                {
                    ExecuteNonQuery(
                        @"IF db_id('TernTests') is not null BEGIN DROP DATABASE [TernTests]; END");
                }
            }

            using (connection = CreateMasterConnection())
            {
                string ldfPath = Path.Combine(Environment.CurrentDirectory, "TernTests_log.ldf");

                ExecuteNonQuery(
                    String.Format(
                        "CREATE DATABASE [TernTests] ON PRIMARY (Name = TernTests, FILENAME = '{0}') LOG ON (Name = TernTests_log, FILENAME = '{1}')",
                        mdfPath, ldfPath));
            }

            this.connection = CreateConnection();
        }

        private DbConnection CreateConnection()
        {
            ConnectionStringSettings connectionString = ConfigurationManager.ConnectionStrings["Tern.TestDb"];

            var provider = DbProviderFactories.GetFactory(connectionString.ProviderName);

            var conn = provider.CreateConnection();

            conn.ConnectionString = connectionString.ConnectionString;

            return conn;
        }

        private DbConnection CreateMasterConnection()
        {
            ConnectionStringSettings connectionString = ConfigurationManager.ConnectionStrings["Tern.TestDb"];

            var provider = DbProviderFactories.GetFactory(connectionString.ProviderName);

            var conn = provider.CreateConnection();

            var builder = provider.CreateConnectionStringBuilder();

            builder.ConnectionString = connectionString.ConnectionString;
            builder["Database"] = "master";
            builder.Remove("AttachDbFilename");

            conn.ConnectionString = builder.ConnectionString;

            return conn;
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
