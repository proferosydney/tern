using Machine.Specifications;
using Profero.Tern.Migrate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Profero.Tern.UnitTests
{
    class VersionVersionStrategySpec
    {
        [Subject(typeof(VersionVersionStrategy))]
        class When_generating_a_numeric_version
        {
            Establish context = () =>
            {
                sut = new VersionVersionStrategy();
                comparisonValue = sut.GetNumericVersion("1.2");
            };

            Because of = () =>
                result = sut.GetNumericVersion("2.1");

            It should_be_relative_to_the_version_number = () =>
                result.ShouldBeGreaterThan(comparisonValue);

            static long result;
            static long comparisonValue;
            static VersionVersionStrategy sut;
        }



        [Subject(typeof(VersionVersionStrategy))]
        class When_comparing_versions_with_different_part_counts
        {
            Establish context = () =>
            {
                sut = new VersionVersionStrategy();
                comparisonValue = sut.GetNumericVersion("1.1.1");
            };

            Because of = () =>
                result = sut.GetNumericVersion("1.1");

            It should_order_by_high_version_parts_first = () =>
                result.ShouldBeLessThan(comparisonValue);

            static long result;
            static long comparisonValue;
            static VersionVersionStrategy sut;
        }

        [Subject(typeof(VersionVersionStrategy))]
        class When_comparing_versions_with_a_single_part
        {
            Establish context = () =>
            {
                sut = new VersionVersionStrategy();
                comparisonValue = sut.GetNumericVersion("1");
            };

            Because of = () =>
                result = sut.GetNumericVersion("2");

            It should_compare_them_as_normal = () =>
                result.ShouldBeGreaterThan(comparisonValue);

            static long result;
            static long comparisonValue;
            static VersionVersionStrategy sut;
        }
    }
}
