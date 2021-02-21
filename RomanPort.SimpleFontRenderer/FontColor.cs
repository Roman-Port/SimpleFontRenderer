using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.SimpleFontRenderer
{
    public struct FontColor
    {
        public float r;
        public float g;
        public float b;

        public FontColor(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public FontColor(float brightness)
        {
            r = brightness;
            g = brightness;
            b = brightness;
        }
    }
}
