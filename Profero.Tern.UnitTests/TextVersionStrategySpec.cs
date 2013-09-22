using Machine.Specifications;
using Profero.Tern.Migrate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Profero.Tern.UnitTests
{
    class TextVersionStrategySpec
    {
        [Subject(typeof(TextVersionStrategy))]
        class When_generating_a_numeric_version
        {
            Establish context = () =>
            {
                sut = new TextVersionStrategy();
            };

            Because of = () =>
                result = sut.GetNumericVersion("asdasd");

            It should_return_zero = () =>
                result.ShouldEqual(0);

            static long result;
            static TextVersionStrategy sut;
        }
    }
}
