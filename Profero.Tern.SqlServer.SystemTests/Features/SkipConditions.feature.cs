﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.0.77
//      SpecFlow Generator Version:1.9.0.0
//      Runtime Version:4.0.30319.18051
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Profero.Tern.SqlServer.SystemTests.Features
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Skip Conditions")]
    public partial class SkipConditionsFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "SkipConditions.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Skip Conditions", "In order to integrate Tern with legacy systems\r\nAs a DBA\r\nI want to be able to sc" +
                    "ript steps but keep their versions", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.TestFixtureTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Script is not run when skip is matched")]
        public virtual void ScriptIsNotRunWhenSkipIsMatched()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Script is not run when skip is matched", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Schema",
                        "Version",
                        "Script",
                        "Skip"});
            table1.AddRow(new string[] {
                        "default",
                        "1.0",
                        "CREATE TABLE [Test] (ID INTEGER NOT NULL, [Name] VARCHAR(32) NULL)",
                        "EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = \'Test\')"});
            table1.AddRow(new string[] {
                        "default",
                        "1.1",
                        "ALTER TABLE [Test] ADD [Name] VARCHAR(32) NULL",
                        "EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = \'Test\' AND COL" +
                            "UMN_NAME = \'Name\')"});
#line 7
 testRunner.Given("the version scripts", ((string)(null)), table1, "Given ");
#line 11
 testRunner.And("I have created an empty database", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 12
 testRunner.When("I apply all the version scripts", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Table",
                        "Column"});
            table2.AddRow(new string[] {
                        "Test",
                        "ID"});
            table2.AddRow(new string[] {
                        "Test",
                        "Name"});
#line 13
 testRunner.Then("the database should contain", ((string)(null)), table2, "Then ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "Schema",
                        "Version"});
            table3.AddRow(new string[] {
                        "default",
                        "1.0"});
            table3.AddRow(new string[] {
                        "default",
                        "1.1"});
#line 17
 testRunner.And("the DatabaseVersion table should contain", ((string)(null)), table3, "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
