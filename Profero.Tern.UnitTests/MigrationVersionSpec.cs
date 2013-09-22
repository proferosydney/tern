using Machine.Specifications;
using Profero.Tern.Migrate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profero.Tern.UnitTests
{
    class MigrationVersionSpec
    {
        class When_attempting_to_sort_an_invalid_version : MigrationVersionContext
        {
            Establish context = () =>
            {
                versioningStyle = VersioningStyle.Version;
                versions = new[]
                {
                    new MigrationVersion("default", "asd", null, null, null),
                    new MigrationVersion("default", "1.2", null, null, null),
                    new MigrationVersion("default", "2.0", null, null, null),
                    new MigrationVersion("default", "1.0.1", null, null, null)
                };
            };

            It should_throw_an_argument_exception = () =>
                exception.ShouldBeOfType<ArgumentException>();
        }

        class When_sorting_versions_as_versions : MigrationVersionContext
        {
            Establish context = () =>
            {
                versioningStyle = VersioningStyle.Version;
                versions = new[]
                {
                    new MigrationVersion("default", "1.0", null, null, null),
                    new MigrationVersion("default", "1.2", null, null, null),
                    new MigrationVersion("default", "2.0", null, null, null),
                    new MigrationVersion("default", "1.0.1", null, null, null)
                };
            };

            It should_sort_them_in_numerical_order = () =>
                result.Select(x => x.Version).ShouldEqual(new[] { "1.0", "1.0.1", "1.2", "2.0" });
        }

        class When_sorting_versions_as_strings : MigrationVersionContext
        {
            Establish context = () =>
            {
                versioningStyle = VersioningStyle.String;
                versions = new[]
                {
                    new MigrationVersion("default", "1.0", null, null, null),
                    new MigrationVersion("default", "1.2", null, null, null),
                    new MigrationVersion("default", "2.0", null, null, null),
                    new MigrationVersion("default", "01.0.1", null, null, null)
                };
            };

            It should_sort_them_in_numerical_order = () =>
                result.Select(x => x.Version).ShouldEqual(new[] { "01.0.1", "1.0", "1.2", "2.0" });
        }

        class When_attempting_to_sort_an_invalid_number : MigrationVersionContext
        {
            Establish context = () =>
            {
                versioningStyle = VersioningStyle.Number;
                versions = new[]
                {
                    new MigrationVersion("default", "asd", null, null, null),
                    new MigrationVersion("default", "1", null, null, null),
                    new MigrationVersion("default", "2", null, null, null),
                    new MigrationVersion("default", "3", null, null, null)
                };
            };

            It should_throw_an_argument_exception = () =>
                exception.ShouldBeOfType<ArgumentException>();
        }

        class When_sorting_versions_as_numbers : MigrationVersionContext
        {
            Establish context = () =>
            {
                versioningStyle = VersioningStyle.Number;
                versions = new []
                {
                    new MigrationVersion("default", "5", null, null, null),
                    new MigrationVersion("default", "2", null, null, null),
                    new MigrationVersion("default", "1", null, null, null),
                    new MigrationVersion("default", "1.5", null, null, null)
                };
            };

            It should_sort_them_in_numerical_order = () =>
                result.Select(x => x.Version).ShouldEqual(new[] { "1", "1.5", "2", "5" });
        }

        class MigrationVersionContext
        {
            protected static VersioningStyle versioningStyle;
            protected static IEnumerable<MigrationVersion> versions;

            Because of = () =>
                exception = Catch.Exception(() => result = MigrationVersion.Sort(versions, new VersionVersionStrategy()));

            protected static Exception exception;
            protected static IEnumerable<MigrationVersion> result;
        }
    }
}
