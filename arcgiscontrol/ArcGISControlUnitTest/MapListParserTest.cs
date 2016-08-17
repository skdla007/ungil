using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArcGISControlUnitTest
{
    using System.Collections.Generic;
    using System.Linq;
    using ArcGISControls.CommonData.Parsers;

    [TestClass]
    public class MapListParserTest
    {
        [TestMethod]
        public void TestEmpty()
        {
            var desiredOutput = new string[]{};
            var result = SplunkMapListParser.ParseMapList("");
            Assert.IsTrue(result == null || desiredOutput.SequenceEqual(result));
            result = SplunkMapListParser.ParseMapList("\" \"");
            Assert.IsTrue(result != null && new []{" "}.SequenceEqual(result), "공백은 공백으로 나와야 한다.");
        }

        [TestMethod]
        public void TestGeneral()
        {
            List<string> result;
            result = SplunkMapListParser.ParseMapList("A,B,C");
            Assert.IsTrue(result != null && new[] { "A", "B", "C" }.SequenceEqual(result));
            result = SplunkMapListParser.ParseMapList("A,\"B,F\",C");
            Assert.IsTrue(result != null && new[] { "A", "B,F", "C" }.SequenceEqual(result));
            result = SplunkMapListParser.ParseMapList("\"A\",\"B,F\",C");
            Assert.IsTrue(result != null && new[] { "A", "B,F", "C" }.SequenceEqual(result));
            result = SplunkMapListParser.ParseMapList("\"\"\"A\",\"B,F\",C");
            Assert.IsTrue(result != null && new[] { "\"A", "B,F", "C" }.SequenceEqual(result));
            result = SplunkMapListParser.ParseMapList(@"으, 아,아,ㅋ");
            Assert.IsTrue(result != null && new[] { "으"," 아","아","ㅋ" }.SequenceEqual(result));
        }
    }
}
