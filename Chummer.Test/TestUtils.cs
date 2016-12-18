using System.Collections.Generic;
using NUnit.Framework;

namespace Chummer.Test
{
    public class TestUtils
    {
        public static void AreMostlyEqual(IEnumerable<double> expected, IEnumerable<double> actuall, double delta)
        {
            bool b1, b2;
            using(var eenum = expected.GetEnumerator())
            using (var aenum = actuall.GetEnumerator())
            {
                while ((b1 = eenum.MoveNext()) & (b2= aenum.MoveNext()))
                {
                    Assert.AreEqual(eenum.Current, aenum.Current, delta);
                }

                if(b1 != b2)
                    Assert.Fail("Lenght differ");
            }
        }
    }
}