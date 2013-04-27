using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ImageProcessing;

namespace ImageProcessingTest {
    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        public void TestMethod1() {
            var result = new List<Color> { new Color(100, 200, 100), new Color(128, 128, 128), new Color(255, 255, 255), new Color(0, 0, 0) };
            result.Sort();
            var actual = new List<Color> { new Color(0, 0, 0), new Color(128, 128, 128), new Color(100, 200, 100), new Color(255, 255, 255) };
            for (int i = 0; i < result.Count; i++) {
                Console.WriteLine("result = " + result[i] + ", actual = " + actual[i]);
                Assert.AreEqual(result[i], actual[i]);
            }
        }
        [TestMethod]
        public void TestImage() {

        }
    }
}
