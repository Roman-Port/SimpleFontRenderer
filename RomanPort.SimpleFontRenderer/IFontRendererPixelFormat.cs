using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.SimpleFontRenderer
{
    public interface IFontRendererPixelFormat
    {
        int BytesPerPixel { get; }
        unsafe void WritePixel(byte* ptr, float r, float g, float b);
    }
}
