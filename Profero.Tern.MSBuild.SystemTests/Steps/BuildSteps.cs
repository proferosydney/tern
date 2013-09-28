using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Microsoft.Build.Execution;
using NUnit.Framework;
using Profero.Tern.MSBuild.SystemTests.Bindings;
using TechTalk.SpecFlow;

namespace Profero.Tern.MSBuild.SystemTests.Steps
{
    [Binding]
    public class BuildSteps
    {
        readonly TestBuildContext testContext;

        public BuildSteps(TestBuildContext testContext)
        {
            this.testContext = testContext;
        }

        [Given(@"I have cleaned the web project")]
        public void GivenIHaveCleanedTheWebProject()
        {
            testContext.BuildTestProject("Clean");
        }

        [Given(@"I have built the web project")]
        [When(@"I build the web project")]
        [When(@"I build the web project again")]
        public void WhenIBuildTheWebProject()
        {
            testContext.BuildTestProject();
        }

        DateTime migrationScriptLastModified;

        [Given(@"recorded the last modified date of the migration script")]
        public void GivenRecordedTheLastModifiedDateOfTheMigrationScript()
        {
            migrationScriptLastModified = File.GetLastWriteTime(testContext.GetMigrationScriptPath("default"));
        }

        [Then(@"the last modified date of the migration script should not change")]
        public void ThenTheLastModifiedDateOfTheMigrationScriptShouldNotChange()
        {
            Assert.AreEqual(
                migrationScriptLastModified,
                File.GetLastWriteTime(testContext.GetMigrationScriptPath("default")));
        }

        [Then(@"the last modified date of the migration script should change")]
        public void ThenTheLastModifiedDateOfTheMigrationScriptShouldChange()
        {
            Assert.AreNotEqual(
                migrationScriptLastModified,
                File.GetLastWriteTime(testContext.GetMigrationScriptPath("default")));
        }

        [When(@"I modify a version script")]
        public void WhenIModifyAVersionScript()
        {
            File.WriteAllText(
                testContext.GetVersionScriptPath("default", "1.2"),
                "--" + Guid.NewGuid().ToString());
        }

        [When(@"I add a version script")]
        public void WhenIAddAVersionScript()
        {
            File.WriteAllText(
                testContext.GetVersionScriptPath("default", "1.3"),
                "--" + Guid.NewGuid().ToString());
        }


        [Then(@"the output directory should contain the migration script")]
        public void ThenTheOutputDirectoryShouldContainTheMigrationScript()
        {
            Assert.IsTrue(File.Exists(testContext.GetMigrationScriptPath("default")));
        }

        [When(@"I package the web project")]
        public void WhenIPackageTheWebProject()
        {
            testContext.BuildProperties["DeployOnBuild"] = "true";
            testContext.BuildProperties["CreatePackageOnPublish"] = "true";

            testContext.BuildTestProject();
        }

        [Then(@"the package should contain a the migration script")]
        public void ThenThePackageShouldContainATheMigrationScript()
        {
            XDocument doc = XDocument.Load(@"WebProject\obj\Debug\Package\TestWebApplication.SourceManifest.xml");

            Assert.IsNotNull(doc.Element("sitemanifest").Element("dbFullSql"));
        }

        private string scriptBackupPath = @"WebProject\DatabaseBackup";

        [BeforeScenario("modifyscripts")]
        public void BackupScripts()
        {
            FileSystemUtility.DeleteDirectory(scriptBackupPath);

            FileSystemUtility.CopyDirectory(@"WebProject\Database", scriptBackupPath);
        }

        [AfterScenario("modifyscripts")]
        public void RestoreScripts()
        {
            FileSystemUtility.DeleteDirectory(@"WebProject\Database");

            FileSystemUtility.CopyDirectory(scriptBackupPath, @"WebProject\Database");
        }

        [Then(@"the package should contain a parameter ""(.*)""")]
        public void ThenThePackageShouldContainAParameter(string parameterName)
        {
            XDocument doc = XDocument.Load(@"WebProject\obj\Debug\Package\TestWebApplication.SetParameters.xml");

            Assert.IsNotNull(doc.Element("parameters")
                .Elements("setParameter")
                .Where(e => e.Attribute("name").Value == parameterName)
                .FirstOrDefault());
        }


        
    }
}
