using Machine.Specifications;
using Profero.Tern.Migrate;
using Profero.Tern.Provider;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Profero.Tern.UnitTests.Provider
{
    /// <summary>
    /// This is an extensibility test, hence testing the base class
    /// </summary>
    class SqlProviderSpec
    {
        class When_generating_with_all_options_enabled 
            : SqlScriptGeneratorContext
        {
            It should_begin_with_a_transaction = () =>
                result.ShouldStartWith("BeginTransaction");

            It should_end_by_committing_the_transaction = () =>
                result.ShouldEndWith("CommitTransaction");

            It should_create_the_version_tracking_storage = () =>
                result.ShouldContain("VersionTrackingStorage");

            It should_check_for_continuity_for_each_version = () =>
                Regex.Matches(result, "VersionContinuity", RegexOptions.Multiline).Count.ShouldEqual(versions.Count);

            It should_check_for_the_skip_condition_for_each_version = () =>
                Regex.Matches(result, "SkipCondition", RegexOptions.Multiline).Count.ShouldEqual(versions.Count);

            It should_apply_each_version = () =>
                Regex.Matches(result, "ApplyVersion", RegexOptions.Multiline).Count.ShouldEqual(versions.Count);

            It should_detech_failed_upgrades_for_each_version = () =>
                Regex.Matches(result, "DetectFailedUpgrade", RegexOptions.Multiline).Count.ShouldEqual(versions.Count);

            It should_track_each_successful_version = () =>
                Regex.Matches(result, "TrackVersion", RegexOptions.Multiline).Count.ShouldEqual(versions.Count);
        }

        class When_transactions_are_disabled
            : SqlScriptGeneratorContext
        {
            Establish context = () =>
            {
                options.UseTransaction = false;
            };

            It should_not_begin_a_transaction = () =>
                result.ShouldNotContain("BeginTransaction");

            It should_not_commit_the_transaction = () =>
                result.ShouldNotContain("CommitTransaction");
        }

        class When_version_tracking_is_disabled
            : SqlScriptGeneratorContext
        {
            Establish context = () =>
            {
                options.TrackVersions = false;
            };

            It should_not_create_the_version_tracking_storage = () =>
                result.ShouldNotContain("VersionTrackingStorage");

            It should_not_track_each_version = () =>
                result.ShouldNotContain("TrackVersion");
        }

        class When_a_version_has_no_skip_condition
            : SqlScriptGeneratorContext
        {
            Establish context = () =>
            {
                versions[0] = new MigrationVersion(versions[0].Schema, versions[0].Version, 
                    versions[0].NumericVersion, versions[0].Description, null, versions[0].Script);
            };

            It should_not_check_for_the_skip_condition_for_that_version = () =>
                Regex.Matches(result, "SkipCondition", RegexOptions.Multiline).Count.ShouldEqual(versions.Count - 1);
        }

        class SqlScriptGeneratorContext
        {
            protected static SqlScriptGenerator scriptGenerator;
            protected static ScriptGenerationOptions options;
            protected static List<MigrationVersion> versions;

            protected static string result;

            Establish context = () =>
            {
                versions = new List<MigrationVersion>
                {
                    new MigrationVersion("default", "1.0", 1, "desc", "skip", "script", "checksum"),
                    new MigrationVersion("default", "1.2", 2, "desc", "skip", "script", "checksum"),
                    new MigrationVersion("default", "1.2", 3, "desc", "skip", "script", "checksum"),
                    new MigrationVersion("default", "1.3", 4, "desc", "skip", "script", "checksum"),
                };

                options = new ScriptGenerationOptions();

                scriptGenerator = new FakeSqlScriptGenerator();
            };

            Because of = () =>
            {
                using (var writer = new StringWriter())
                {
                    scriptGenerator.Generate(versions, writer, options);
                    result = writer.ToString().Trim();
                }
            };
        }

        class FakeSqlScriptGenerator : SqlScriptGenerator
        {
            protected override void GenerateScriptForApplyingVersionScript(Migrate.MigrationVersion version, Migrate.ScriptGenerationOptions options, System.IO.TextWriter output)
            {
                output.WriteLine("ApplyVersion");
            }

            protected override void GenerateScriptForBeginTransaction(System.IO.TextWriter output)
            {
                output.WriteLine("BeginTransaction");
            }

            protected override void GenerateScriptForCommitTransaction(System.IO.TextWriter output)
            {
                output.WriteLine("CommitTransaction");
            }

            protected override void GenerateScriptForDetectingFailedUpgrade(Migrate.MigrationVersion version, Migrate.ScriptGenerationOptions options, System.IO.TextWriter output)
            {
                output.WriteLine("DetectFailedUpgrade");
            }

            protected override void GenerateScriptForSkipCondition(Migrate.MigrationVersion version, Migrate.ScriptGenerationOptions options, System.IO.TextWriter output)
            {
                output.Write("SkipCondition");
            }

            protected override void GenerateScriptForVersionContinuityChecks(Migrate.MigrationVersion version, string tableName, bool rollbackTransactionOnError, System.IO.TextWriter output)
            {
                output.WriteLine("VersionContinuity");
            }

            protected override void GenerateScriptForVersionTrackingStorage(string tableName, System.IO.TextWriter output)
            {
                output.WriteLine("VersionTrackingStorage");
            }

            protected override void GenerateScriptToTrackVersion(Migrate.MigrationVersion version, Migrate.ScriptGenerationOptions options, System.IO.TextWriter output)
            {
                output.WriteLine("TrackVersion");
            }
        }
    }
}
