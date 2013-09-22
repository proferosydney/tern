using Machine.Specifications;
using Profero.Tern.Migrate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Profero.Tern.UnitTests
{
    class DateVersionStrategySpec
    {
        [Subject(typeof(DateVersionStrategy))]
        class When_generating_a_numeric_version
        {
            Establish context = () =>
            {
                sut = new DateVersionStrategy();
                comparisonValue = sut.GetNumericVersion("2013-09-22");
            };

            Because of = () =>
                result = sut.GetNumericVersion("2013-09-23");

            It should_be_relative_to_the_version_number = () =>
                result.ShouldBeGreaterThan(comparisonValue);

            static long result;
            static long comparisonValue;
            static DateVersionStrategy sut;
        }

        [Subject(typeof(DateVersionStrategy))]
        class When_comparing_versions_without_revisions
        {
            Establish context = () =>
            {
                sut = new DateVersionStrategy();
                comparisonValue = sut.GetNumericVersion("2013-09-22-01");
            };

            Because of = () =>
                result = sut.GetNumericVersion("2013-09-22");

            It should_assume_no_revision_is_zero = () =>
                result.ShouldBeLessThan(comparisonValue);

            static long result;
            static long comparisonValue;
            static DateVersionStrategy sut;
        }

        [Subject(typeof(DateVersionStrategy))]
        class When_comparing_versions_with_revisions
        {
            Establish context = () =>
            {
                sut = new DateVersionStrategy();
                comparisonValue = sut.GetNumericVersion("2013-09-22-01");
            };

            Because of = () =>
                result = sut.GetNumericVersion("2013-09-22-02");

            It should_order_by_revision_then_date = () =>
                result.ShouldBeGreaterThan(comparisonValue);

            static long result;
            static long comparisonValue;
            static DateVersionStrategy sut;
        }

        [Subject(typeof(DateVersionStrategy))]
        class When_an_invalid_date_is_used
        {
            Establish context = () =>
            {
                sut = new DateVersionStrategy();
            };

            Because of = () =>
                result = Catch.Exception(() => sut.GetNumericVersion("abc"));

            It should_throw_a_FormatException = () =>
                result.ShouldBeOfType<FormatException>();

            static Exception result;
            static DateVersionStrategy sut;
        }
    }
}
