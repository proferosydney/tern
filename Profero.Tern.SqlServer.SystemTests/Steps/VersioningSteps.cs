using NUnit.Framework;
using Profero.Tern.Migrate;
using Profero.Tern.SqlServer.SystemTests.Bindings;
using System;
using System.Linq;
using System.Data;
using System.IO;
using TechTalk.SpecFlow;
using System.Data.SqlClient;

namespace Profero.Tern.SqlServer.SystemTests.Steps
{
    [Binding]
    public class VersioningSteps
    {
        readonly TestDataContext dataContext;

        public VersioningSteps(TestDataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        [Given(@"the version scripts")]
        [When(@"I add the version scripts")]
        public void GivenTheVersionScripts(Table table)
        {
            foreach (var row in table.Rows)
            {
                string schema = row["Schema"];
                string version = row["Version"];
                string script = row["Script"];
                string skip = null;

                if (table.ContainsColumn("Skip"))
                {
                    skip = row["Skip"];
                }

                dataContext.Versions.Add(new Migrate.MigrationVersion(schema, version, 
                    dataContext.VersionStrategy.GetNumericVersion(version), null, skip, script));
            }

            dataContext.SortVersions();
        }

        [When(@"I modify the Script")]
        public void WhenIModifyTheScript(Table table)
        {
            foreach (var row in table.Rows)
            {
                string schema = row["Schema"];
                string version = row["Version"];
                string script = row["Script"];

                var migrationVersion = dataContext.Versions.First(x => x.Schema == schema && x.Version == version);

                dataContext.Versions.Remove(migrationVersion);

                dataContext.Versions.Add(new MigrationVersion(migrationVersion.Schema, 
                    migrationVersion.Version, migrationVersion.NumericVersion, migrationVersion.Description, 
                    migrationVersion.SkipCondition, script));

                dataContext.SortVersions();
            }
        }

        [Given(@"I have disabled tracking")]
        public void GivenIHaveDisabledTracking()
        {
            dataContext.Options.TrackVersions = false;
        }

        [Given(@"I have disabled transactions")]
        public void GivenIHaveDisabledTransactions()
        {
            dataContext.Options.UseTransaction = false;
        }



        [Given(@"I have applied the version scripts")]
        public void GivenIHaveAppliedTheVersionScripts(Table table)
        {
            GivenTheVersionScripts(table);
            WhenIApplyAllTheVersionScripts();
        }

        
        [Given(@"I have created an empty database")]
        public void GivenIHaveCreatedAnEmptyDatabase()
        {
            dataContext.CreateDatabase();
        }
        
        [When(@"I apply all the version scripts")]
        [When(@"I apply all the version scripts again")]
        public void WhenIApplyAllTheVersionScripts()
        {
            StringWriter writer = new StringWriter();

            dataContext.DatabaseProvider.Generate(dataContext.Versions, writer, dataContext.Options);

            dataContext.ExecuteNonQuery(writer.ToString());
        }
        
        [Then(@"the database should contain")]
        public void ThenTheDatabaseShouldContain(Table table)
        {
            IDataReader reader = dataContext.Execute("SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS");

            var dt = new DataTable();
            dt.Load(reader);

            foreach (var row in table.Rows)
            {
                string query = "TABLE_NAME='" + row["Table"] + "'";

                if (table.ContainsColumn("Column"))
                {
                    query += " AND COLUMN_NAME ='" + row["Column"] + "'";
                }

                Assert.AreNotEqual(0, dt.Select(query).Length);
            }
        }

        [Then(@"the database should not contain")]
        public void ThenTheDatabaseShouldNotContain(Table table)
        {
            IDataReader reader = (table.ContainsColumn("Column"))
                ? dataContext.Execute("SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS")
                : dataContext.Execute("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES");

            var dt = new DataTable();
            dt.Load(reader);

            foreach (var row in table.Rows)
            {
                string query = "TABLE_NAME='" + row["Table"] + "'";

                if (table.ContainsColumn("Column"))
                {
                    query += " AND COLUMN_NAME ='" + row["Column"] + "'";
                }

                Assert.AreEqual(0, dt.Select(query).Length);
            }
        }


        [Then(@"the (.*) table should contain")]
        public void ThenTheTableShouldContain(string tableName, Table table)
        {
            string query = "SELECT [" + String.Join("], [", table.Header) + "] FROM [" + tableName + "]";

            var reader = dataContext.Execute(query);

            var dt = new DataTable();
            dt.Load(reader);

            foreach (var row in table.Rows)
            {
                string expr = String.Join(" AND ", table.Header.Select(h => "`" + h + "`" + "='" + row[h] + "'"));

                Assert.AreNotEqual(0, dt.Select(expr).Length, 
                    String.Format("Could not find row in {0} with {1}", tableName, expr));
            }
        }

        [Then(@"the (.*) table should contain (.*) rows")]
        public void ThenTheDatabaseVersionTableShouldContainRows(string tableName, int rowCount)
        {
            string query = "SELECT COUNT(*) FROM [" + tableName + "]";

            using (var reader = dataContext.Execute(query))
            {
                reader.Read();

                int actualRowCount = reader.GetInt32(0);

                Assert.AreEqual(rowCount, actualRowCount,
                    "Unexpected number of rows in " + tableName);
            }
        }

        [Then(@"attempting to apply the version scripts again should fail")]
        public void ThenAttemptingToApplyTheVersionScriptsAgainShouldFail()
        {
            try
            {
                WhenIApplyAllTheVersionScripts();

                throw new InvalidOperationException("Expected script application to fail");
            }
            catch (SqlException)
            {
            }
        }

    }
}
