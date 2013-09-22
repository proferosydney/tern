using Machine.Specifications;
using Profero.Tern.Migrate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profero.Tern.UnitTests
{
    public class MigrationVersionFactorySpec
    {
        [Subject(typeof(MigrationVersionFactory), "Create")]
        class When_creating_a_version_from_a_file : MigrationVersionFactoryContext
        {
            Establish context = () =>
            {
                testScript = new TestVersionScript("2013-09-21.sql", "test1", "test2", "test3");
                options = new MigrationScriptOptions
                {
                    VersioningStyle = VersioningStyle.Date
                };
            };

            Because of = () =>
                result = sut.Create(testScript.CreateReader(), testScript.Filename, options);

            It should_extract_the_version_from_the_filename = () =>
                result.Version.ShouldEqual("2013-09-21");

            It should_extract_the_description_from_the_script = () =>
                result.Description.ShouldEqual("test1");

            It should_extract_the_skip_condition_from_the_script = () =>
                result.SkipCondition.ShouldEqual("test2");

            It should_extract_the_script_contents = () =>
                result.Script.ShouldContain("test3");

            It should_not_include_the_description_in_the_script_contents = () =>
                result.Script.ShouldNotContain("test1");

            It should_not_include_the_skip_condition_in_the_script_contents = () =>
                result.Script.ShouldNotContain("test2");

            static TestVersionScript testScript;
            static MigrationScriptOptions options;
            static MigrationVersion result;
        }

        [Subject(typeof(MigrationVersionFactory), "Create")]
        class When_creating_a_version_from_a_file_with_no_description : MigrationVersionFactoryContext
        {
            Establish context = () =>
            {
                testScript = new TestVersionScript("2013-09-21.sql", null, "test2", "test3");
                options = new MigrationScriptOptions
                {
                    VersioningStyle = VersioningStyle.Date
                };
            };

            Because of = () =>
                result = sut.Create(testScript.CreateReader(), testScript.Filename, options);

            It should_not_specify_a_description = () =>
                result.Description.ShouldBeNull();

            It should_extract_the_skip_condition_from_the_script = () =>
                result.SkipCondition.ShouldEqual("test2");

            static TestVersionScript testScript;
            static MigrationScriptOptions options;
            static MigrationVersion result;
        }

        [Subject(typeof(MigrationVersionFactory), "Create")]
        class When_creating_a_version_from_a_file_with_no_skip_condition : MigrationVersionFactoryContext
        {
            Establish context = () =>
            {
                testScript = new TestVersionScript("2013-09-21.sql", "test1", null, "test3");
                options = new MigrationScriptOptions
                {
                    VersioningStyle = VersioningStyle.Date
                };
            };

            Because of = () =>
                result = sut.Create(testScript.CreateReader(), testScript.Filename, options);

            It should_not_specify_a_skip_condition = () =>
                result.SkipCondition.ShouldBeNull();

            It should_extract_the_description_from_the_script = () =>
                result.Description.ShouldEqual("test1");

            static TestVersionScript testScript;
            static MigrationScriptOptions options;
            static MigrationVersion result;
        }

        [Subject(typeof(MigrationVersionFactory), "Create")]
        class When_creating_a_version_from_a_file_with_no_metadata : MigrationVersionFactoryContext
        {
            Establish context = () =>
            {
                testScript = new TestVersionScript("2013-09-21.sql", null, null, "test3");
                options = new MigrationScriptOptions
                {
                    VersioningStyle = VersioningStyle.Date
                };
            };

            Because of = () =>
                result = sut.Create(testScript.CreateReader(), testScript.Filename, options);

            It should_not_specify_a_skip_condition = () =>
                result.SkipCondition.ShouldBeNull();

            It should_not_specify_a_description = () =>
                result.Description.ShouldBeNull();

            static TestVersionScript testScript;
            static MigrationScriptOptions options;
            static MigrationVersion result;
        }

        [Subject(typeof(MigrationVersionFactory), "CanCreate")]
        class When_checking_if_a_non_matching_filename_can_create : MigrationVersionFactoryContext
        {
            Establish context = () =>
            {
                options = new MigrationScriptOptions();
            };

            Because of = () =>
                result = sut.CanCreate("notmatching.sql", options);

            It should_return_false = () =>
                result.ShouldBeFalse();

            static MigrationScriptOptions options;
            static bool result;
        }

        [Subject(typeof(MigrationVersionFactory), "Create")]
        class When_creating_a_version_from_a_non_matching_filename : MigrationVersionFactoryContext
        {
            Establish context = () =>
            {
                testScript = new TestVersionScript("notmatching.sql", null, null, null);
                options = new MigrationScriptOptions();
            };

            Because of = () =>
                result = Catch.Exception(() => sut.Create(testScript.CreateReader(), testScript.Filename, options));

            It should_throw_an_exception = () =>
                result.ShouldBeOfType<ArgumentException>();

            static TestVersionScript testScript;
            static MigrationScriptOptions options;
            static Exception result;
        }

        class MigrationVersionFactoryContext
        {
            protected static MigrationVersionFactory sut;

            Establish context = () =>
            {
                sut = new MigrationVersionFactory();
            };
        }
    }
}
