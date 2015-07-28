﻿/* Copyright (c) 2006-2008, Peter Golde
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without 
 * modification, are permitted provided that the following conditions are 
 * met:
 * 
 * 1. Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 * 
 * 2. Redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution.
 * 
 * 3. Neither the name of Peter Golde, nor "Purple Pen", nor the names
 * of its contributors may be used to endorse or promote products
 * derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE
 * USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
 * OF SUCH DAMAGE.
 */

#if TEST
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using TestingUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PurplePen.Tests
{
    [TestClass]
    public class MapUtilTests : TestFixtureBase
    {

        [TestMethod]
        public void ValidateMapFileOCAD()
        {
            float scale, dpi;
            Size bitmapSize;
            RectangleF mapBounds;
            MapType mapType;
            string errorMessageText;
            bool result;

            result = MapUtil.ValidateMapFile(TestUtil.GetTestFile("mapdisplay\\SampleEvent.ocd"), out scale, out dpi, out bitmapSize, out mapBounds, out mapType, out errorMessageText);
            Assert.IsTrue(result);
            Assert.AreEqual(MapType.OCAD, mapType);
            Assert.AreEqual(15000, scale);
            Assert.AreEqual(-0.22F, mapBounds.Left, 0.01F);
            Assert.AreEqual(0.01F, mapBounds.Top, 0.01F);
            Assert.AreEqual(200.07F, mapBounds.Width, 0.01F);
            Assert.AreEqual(267.79F, mapBounds.Height, 0.01F);

            result = MapUtil.ValidateMapFile(TestUtil.GetTestFile("mapdisplay\\overprint.ocd"), out scale, out dpi, out bitmapSize, out mapBounds, out mapType, out errorMessageText);
            Assert.IsTrue(result);
            Assert.AreEqual(MapType.OCAD, mapType);
            Assert.AreEqual(10000, scale);
            Assert.AreEqual(36.75F, mapBounds.Left, 0.01F);
            Assert.AreEqual(169.43F, mapBounds.Top, 0.01F);
            Assert.AreEqual(112.77F, mapBounds.Right, 0.01F);
            Assert.AreEqual(214.96F, mapBounds.Bottom, 0.01F);

        }

        [TestMethod]
        public void ValidateMapFileBitmap()
        {
            float scale, dpi;
            Size bitmapSize;
            RectangleF mapBounds;
            MapType mapType;
            string errorMessageText;
            bool result;

            result = MapUtil.ValidateMapFile(TestUtil.GetTestFile("mapdisplay\\SampleEvent.jpg"), out scale, out dpi, out bitmapSize, out mapBounds, out mapType, out errorMessageText);
            Assert.IsTrue(result);
            Assert.AreEqual(MapType.Bitmap, mapType);
            Assert.AreEqual(96, dpi, 0.1F);
            Assert.AreEqual(0F, mapBounds.Left, 0.01F);
            Assert.AreEqual(0F, mapBounds.Top, 0.01F);
            Assert.AreEqual(628.39F, mapBounds.Width, 0.01F);
            Assert.AreEqual(841.11F, mapBounds.Height, 0.01F);
        }


        [TestMethod]
        public void ValidateMapFilePDF()
        {
            float scale, dpi;
            Size bitmapSize;
            RectangleF mapBounds;
            MapType mapType;
            string errorMessageText;
            bool result;

            result = MapUtil.ValidateMapFile(TestUtil.GetTestFile("pdfmaps\\Potholes.pdf"), out scale, out dpi, out bitmapSize, out mapBounds, out mapType, out errorMessageText);
            Assert.IsTrue(result);
            Assert.AreEqual(MapType.PDF, mapType);
            Assert.AreEqual(600, dpi, 0.1F);
            Assert.AreEqual(0F, mapBounds.Left, 0.01F);
            Assert.AreEqual(0F, mapBounds.Top, 0.01F);
            Assert.AreEqual(215.9F, mapBounds.Width, 0.01F);
            Assert.AreEqual(279.4F, mapBounds.Height, 0.01F);
        }
    }
}

#endif //TEST
