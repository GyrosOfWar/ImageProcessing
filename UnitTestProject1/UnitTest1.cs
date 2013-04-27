using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ImageProcessing;

namespace ImageProcessingTest {
    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        public void TestImage() {
            ColorImage img = new ColorImage(15, 15);
            for(int i = 0; i < 15; i++) {
                for(int j = 0; j < 15; j++) {
                    img.Red[i, j] = 5;
                }
            }
            var result = ColorImage.GetNeighborhood(img, 1, 1, 3);
            var actual = new byte[] { 5, 5, 5, 5, 5, 5, 5, 5, 5};
            int k = 0;
            foreach(var b in result)
                Assert.AreEqual(b, actual[k++]);
            k = 0;
            foreach(var b in actual)
                Assert.AreEqual(b, result[k++]);


        }
    }
}
