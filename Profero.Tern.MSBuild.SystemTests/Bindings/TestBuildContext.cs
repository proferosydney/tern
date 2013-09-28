using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Execution;
using NUnit.Framework;

namespace Profero.Tern.MSBuild.SystemTests.Bindings
{
    public class TestBuildContext
    {
        private Dictionary<string, string> properties = new Dictionary<string, string>()
        {
            {"Configuration", "Debug"},
            {"Platform", "AnyCPU"}
        };

        public string TestProjectFilePath
        {
            get
            {
                return Path.Combine(Environment.CurrentDirectory, "WebProject\\TestWebApplication.csproj");
            }
        }

        public IDictionary<string, string> BuildProperties
        {
            get { return properties; }
        }

        public void BuildTestProject()
        {
            BuildTestProject("Build");
        }

        public void BuildTestProject(string target)
        {
            properties["TernToolsPath"] = Environment.CurrentDirectory;

            var buildRequestData = new BuildRequestData(TestProjectFilePath,
                properties, null, new[] { target }, null);

            var buildParameters = new BuildParameters(
                new Microsoft.Build.Evaluation.ProjectCollection()
                );

            var logger = new StringLogger();

            buildParameters.Loggers = new[] {logger};

            var result = BuildManager.DefaultBuildManager.Build(buildParameters, buildRequestData);

            Assert.AreEqual(BuildResultCode.Success, result.OverallResult, logger.ToString());
        }

        public string GetVersionScriptPath(string schema, string version)
        {
            return String.Format(@"WebProject\Database\{0}\{1}.sql", schema, version);
        }

        public string GetMigrationScriptPath(string schema)
        {
            return String.Format(@"WebProject\obj\Debug\{0}-migrate.sql", schema);
        }
    }
}
