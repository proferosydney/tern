using Machine.Fakes;
using Machine.Specifications;
using Profero.Tern.Migrate;
using Profero.Tern.Provider;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Profero.Tern.UnitTests
{
    class MigrationScriptGeneratorSpec
    {
        [Subject(typeof(MigrationScriptGenerator))]
        class When_generating_a_migration_script : MigrationScriptGeneratorContext
        {
            It should_bootstrap_the_version_table = () =>
                result.ShouldContain("CreateTable");

            It should_begin_with_a_transaction = () =>
                result.ShouldStartWith("BeginTransaction");

            It should_end_by_committing_a_transaction = () =>
                result.TrimEnd().ShouldEndWith("CommitTransaction");

            It should_script_each_version = () =>
                result.ShouldContain("1.0");

            It should_include_a_hash_for_each_version = () =>
                result.ShouldContain("5a105e8b9d40e1329780d62ea2265d8a");
        }

        [Subject(typeof(MigrationScriptGenerator))]
        class When_transactions_are_disabled : MigrationScriptGeneratorContext
        {
            Establish context = () =>
            {
                options.UseTransaction = false;
            };

            It should_not_output_transaction_statements = () =>
            {
                result.ShouldNotContain("BeginTransaction");
                result.ShouldNotContain("EndTransaction");
            };
        }

        [Subject(typeof(MigrationScriptGenerator))]
        class When_version_tracking_is_disabled : MigrationScriptGeneratorContext
        {
            Establish context = () =>
            {
                options.TrackVersions = false;
            };

            It should_not_create_the_version_table = () =>
                result.ShouldNotContain("CreateTable");
        }

        class MigrationScriptGeneratorContext : WithFakes
        {
            protected static IEnumerable<MigrationVersion> versions;
            protected static MigrationScriptGenerator generator;
            protected static TextWriter output;
            protected static IDatabaseScriptGenerator databaseProvider;
            protected static ScriptGenerationOptions options;
            protected static string result;

            Establish context = () =>
            {
                generator = new MigrationScriptGenerator();

                output = new StringWriter();

                databaseProvider = An<IDatabaseScriptGenerator>();

                options = new ScriptGenerationOptions();

                databaseProvider.WhenToldTo(p => p.GenerateScriptForVersionTrackingStorage(
                    options.VersionTableName,
                    output)
                    )
                    .Callback<string, TextWriter>((dt, tw) => tw.WriteLine("CreateTable"));

                databaseProvider.WhenToldTo(p => p.GenerateScriptForBeginTransaction(
                    output)
                    )
                    .Callback<TextWriter>((tw) => tw.WriteLine("BeginTransaction"));

                databaseProvider.WhenToldTo(p => p.GenerateScriptForCommitTransaction(
                    output)
                    )
                    .Callback<TextWriter>((tw) => tw.WriteLine("CommitTransaction"));

                databaseProvider.WhenToldTo(p => p.MigrateVersion(
                        Param<MigrationVersion>.IsAnything,
                        output,
                        Param<ScriptGenerationOptions>.IsAnything)
                    )
                    .Callback<MigrationVersion, TextWriter, ScriptGenerationOptions>((version, o, opt) => o.WriteLine("{0}|{1}", version.Version, version.Checksum));

                versions = new[]
                {
                    new MigrationVersion("", "1.0", 1, "", "", "test1"),
                    new MigrationVersion("", "2.0", 2, "", "", "test2"),
                    new MigrationVersion("", "3.0", 3, "", "", "test3")
                };
            };

            Because of = () =>
            {
                generator.Generate(versions, databaseProvider, output, options);
                result = output.ToString();
            };
        }
    }
}
