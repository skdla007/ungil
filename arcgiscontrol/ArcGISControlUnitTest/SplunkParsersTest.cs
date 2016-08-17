using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArcGISControlUnitTest
{
    using System;
    using ArcGISControls.CommonData.Parsers;

    [TestClass]
    public class SplunkParsersTest
    {
        [TestMethod]
        public void ParseLineInfo()
        {
            var result = SplunkLineToParser.ParseLineInfo(@"(123,456),(.432,0.5389)");
            Assert.IsTrue(Math.Abs(result.Item1 - 123) < 1e-15);
            Assert.IsTrue(Math.Abs(result.Item2 - 456) < 1e-15);
            Assert.IsTrue(Math.Abs(result.Item3 - .432) < 1e-15);
            Assert.IsTrue(Math.Abs(result.Item4 - 0.5389) < 1e-15);
            result = SplunkLineToParser.ParseLineInfo(@"(123,456),(1e-9,1e+8)");
            Assert.IsTrue(Math.Abs(result.Item1 - 123) < 1e-15);
            Assert.IsTrue(Math.Abs(result.Item2 - 456) < 1e-15);
            Assert.IsTrue(Math.Abs(result.Item3 - 1e-9) < 1e-15);
            Assert.IsTrue(Math.Abs(result.Item4 - 1e+8) < 1e-15);
            result = SplunkLineToParser.ParseLineInfo(@"(,456),(1e-9,1e+8)");
            Assert.IsTrue(result == null);
            result = SplunkLineToParser.ParseLineInfo(@"1,456,1e-9,1e+8");
            Assert.IsTrue(result == null);
        }
    }
}
