using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.SimpleFontRenderer.PixelFormats
{
    public class BgraPixelFormat : IFontRendererPixelFormat
    {
        public int BytesPerPixel => 4;

        public unsafe void WritePixel(byte* ptr, float r, float g, float b)
        {
            ptr[2] = (byte)(r * byte.MaxValue);
            ptr[1] = (byte)(g * byte.MaxValue);
            ptr[0] = (byte)(b * byte.MaxValue);
        }
    }
}
