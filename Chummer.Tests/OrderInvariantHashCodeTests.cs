/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Chummer.Tests
{
    [TestClass]
    public class OrderInvariantHashCodeTests
    {
        const string dummyData1 = "Lorem ipsum dolor sit amet.";
        const string dummyData2 = "Consectetur adipiscing elit.";
        const string dummyData3 = "Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
        const string dummyData4 = "Ut enim ad minim veniam.";
        const string dummyData5 = "Quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.";
        const string dummyData6 = "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.";
        const string dummyData7 = "Excepteur sint occaecat cupidatat non proident.";
        const string dummyData8 = "Sunt in culpa qui officia deserunt mollit anim id est laborum.";

        [TestMethod]
        public void EmptyOrderInvariantHashCodeMatchesEmptyHashCode()
        {
            HashCode hashCode = new HashCode();
            OrderInvariantHashCode invariantHashCode = new OrderInvariantHashCode();
            int emptyHashCode1 = hashCode.ToHashCode();
            int emptyHashCode2 = invariantHashCode.ToHashCode();
            Assert.AreEqual(emptyHashCode1, emptyHashCode2);
        }

        [TestMethod]
        public void OneElementOrderInvariantHashCodeMatchesOneElementHashCode()
        {
            OrderInvariantHashCode invariantHashCode = new OrderInvariantHashCode();
            invariantHashCode.Add(dummyData1);
            int hashCode1 = HashCode.Combine(dummyData1);
            int hashCode2 = invariantHashCode.ToHashCode();
            Assert.AreEqual(hashCode1, hashCode2);
        }

        [TestMethod]
        public void OrderInvariantHashCodeCombineMatchesAdd()
        {
            OrderInvariantHashCode invariantHashCode = new OrderInvariantHashCode();
            invariantHashCode.Add(dummyData1);
            Assert.AreEqual(OrderInvariantHashCode.Combine(dummyData1), invariantHashCode.ToHashCode());
            TestContext.CancellationToken.ThrowIfCancellationRequested();
            invariantHashCode.Add(dummyData2);
            Assert.AreEqual(OrderInvariantHashCode.Combine(dummyData1, dummyData2), invariantHashCode.ToHashCode());
            TestContext.CancellationToken.ThrowIfCancellationRequested();
            invariantHashCode.Add(dummyData3);
            Assert.AreEqual(OrderInvariantHashCode.Combine(dummyData1, dummyData2, dummyData3), invariantHashCode.ToHashCode());
            TestContext.CancellationToken.ThrowIfCancellationRequested();
            invariantHashCode.Add(dummyData4);
            Assert.AreEqual(OrderInvariantHashCode.Combine(dummyData1, dummyData2, dummyData3, dummyData4), invariantHashCode.ToHashCode());
            TestContext.CancellationToken.ThrowIfCancellationRequested();
            invariantHashCode.Add(dummyData5);
            Assert.AreEqual(OrderInvariantHashCode.Combine(dummyData1, dummyData2, dummyData3, dummyData4, dummyData5), invariantHashCode.ToHashCode());
            TestContext.CancellationToken.ThrowIfCancellationRequested();
            invariantHashCode.Add(dummyData6);
            Assert.AreEqual(OrderInvariantHashCode.Combine(dummyData1, dummyData2, dummyData3, dummyData4, dummyData5, dummyData6), invariantHashCode.ToHashCode());
            TestContext.CancellationToken.ThrowIfCancellationRequested();
            invariantHashCode.Add(dummyData7);
            Assert.AreEqual(OrderInvariantHashCode.Combine(dummyData1, dummyData2, dummyData3, dummyData4, dummyData5, dummyData6, dummyData7), invariantHashCode.ToHashCode());
            TestContext.CancellationToken.ThrowIfCancellationRequested();
            invariantHashCode.Add(dummyData8);
            Assert.AreEqual(OrderInvariantHashCode.Combine(dummyData1, dummyData2, dummyData3, dummyData4, dummyData5, dummyData6, dummyData7, dummyData8), invariantHashCode.ToHashCode());
            TestContext.CancellationToken.ThrowIfCancellationRequested();
        }

        [TestMethod]
        public void OrderInvariantHashCodeIsOrderInvariant()
        {
            // Test commutative
            int hashCode1 = OrderInvariantHashCode.Combine(dummyData1, dummyData2);
            int hashCode2 = OrderInvariantHashCode.Combine(dummyData2, dummyData1);
            Assert.AreEqual(hashCode1, hashCode2);

            TestContext.CancellationToken.ThrowIfCancellationRequested();

            // Test associative
            OrderInvariantHashCode invariantHashCode1 = new OrderInvariantHashCode();
            invariantHashCode1.Add(dummyData1);
            invariantHashCode1.Add(dummyData2);
            invariantHashCode1.Add(dummyData3);
            OrderInvariantHashCode invariantHashCode2 = new OrderInvariantHashCode();
            invariantHashCode2.Add(dummyData2);
            invariantHashCode2.Add(dummyData3);
            invariantHashCode2.Add(dummyData1);
            hashCode1 = invariantHashCode1.ToHashCode();
            hashCode2 = invariantHashCode2.ToHashCode();
            Assert.AreEqual(hashCode1, hashCode2);

            TestContext.CancellationToken.ThrowIfCancellationRequested();

            // Test associative (parallelized)
            List<string> lstMiddleData = new List<string>
            {
                dummyData2,
                dummyData3,
                dummyData4,
                dummyData5,
                dummyData6,
                dummyData7
            };
            OrderInvariantHashCode invariantHashCode3 = new OrderInvariantHashCode();
            invariantHashCode3.Add(dummyData1);
            invariantHashCode3.AddRange(lstMiddleData);
            invariantHashCode3.Add(dummyData8);
            OrderInvariantHashCode invariantHashCode4 = new OrderInvariantHashCode();
            invariantHashCode4.Add(dummyData8);
            invariantHashCode4.AddRangeParallel(lstMiddleData, TestContext.CancellationToken);
            invariantHashCode4.Add(dummyData1);
            hashCode1 = invariantHashCode3.ToHashCode();
            hashCode2 = invariantHashCode4.ToHashCode();
            Assert.AreEqual(hashCode1, hashCode2);
        }

        public TestContext TestContext { get; set; }
    }
}
